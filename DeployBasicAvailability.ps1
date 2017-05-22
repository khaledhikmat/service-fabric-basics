$clusterUrl = "localhost"
$imageStoreConnectionString = "file:C:\SfDevCluster\Data\ImageStoreShare"   # Use this with OneBox
#$imageStoreConnectionString = "fabric:ImageStore"                          # Use this when not using

$appPkgName = "BasicAvailabilityAppTypePkg"

$appTypeName = "BasicAvailabilityAppType"
$appName = "fabric:/BasicAvailabilityApp"

$serviceTypeName = "CrashableServiceType"
$serviceName = $appName + "/CrashableService"

# Connect PowerShell session to a cluster
Connect-ServiceFabricCluster -ConnectionEndpoint ${clusterUrl}:19000

# Copy the application package to the cluster
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath "BasicAvailabilityApp" -ImageStoreConnectionString $imageStoreConnectionString -ApplicationPackagePathInImageStore $appPkgName

# Register the application package's application type/version
Register-ServiceFabricApplicationType -ApplicationPathInImageStore $appPkgName

# After registering the package's app type/version, you can remove the package
Remove-ServiceFabricApplicationPackage -ImageStoreConnectionString $imageStoreConnectionString -ApplicationPackagePathInImageStore $appPkgName

# Create a named application from the registered app type/version
New-ServiceFabricApplication -ApplicationTypeName $appTypeName -ApplicationTypeVersion "Beta" -ApplicationName $appName 

# Create a named service within the named app from the service's type
New-ServiceFabricService -ApplicationName $appName -ServiceTypeName $serviceTypeName -ServiceName $serviceName -Stateless -PartitionSchemeSingleton -InstanceCount 1



# Copy the application package ProductionV1 to the cluster
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath "BasicAvailabilityApp-ProductionV1" -ImageStoreConnectionString $imageStoreConnectionString -ApplicationPackagePathInImageStore $appPkgName

# Register the application package's application type/version
Register-ServiceFabricApplicationType -ApplicationPathInImageStore $appPkgName

# After registering the package's app type/version, you can remove the package
Remove-ServiceFabricApplicationPackage -ImageStoreConnectionString $imageStoreConnectionString -ApplicationPackagePathInImageStore $appPkgName

# Upgrade the application from Beta to ProductionV1
Start-ServiceFabricApplicationUpgrade -ApplicationName $appName -ApplicationTypeVersion "ProductionV1" -UnmonitoredAuto -UpgradeReplicaSetCheckTimeoutSec 100


# Dynamically change the named service's number of instances
Update-ServiceFabricService -ServiceName $serviceName -Stateless -InstanceCount 5 -Force

# Dynamically change the named service's number of instances back to 1
Update-ServiceFabricService -ServiceName $serviceName -Stateless -InstanceCount 1 -Force


# Delete the named service
Remove-ServiceFabricService -ServiceName $serviceName -Force

# Delete the named application (deletes all its named services)
Remove-ServiceFabricApplication -ApplicationName $appName -Force

# If no named apps are running, you can delete the app type/version
Unregister-ServiceFabricApplicationType -ApplicationTypeName $appTypeName -ApplicationTypeVersion "Beta" -Force
Unregister-ServiceFabricApplicationType -ApplicationTypeName $appTypeName -ApplicationTypeVersion "ProductionV1" -Force
