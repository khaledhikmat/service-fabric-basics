using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Threading;
using System.Threading.Tasks;

/*
 * Credit to Jeff Richter - Azure Service Fabric Team 
 */ 
namespace MonitorService
{
   public sealed class HealthMonitor : IEnumerable<KeyValuePair<String, HealthCheckInfo>> {
      private readonly FabricClient m_fabricClient;
      private readonly ConcurrentDictionary<String, HealthCheckInfo> m_healthChecks
         = new ConcurrentDictionary<string, HealthCheckInfo>();
      public readonly String SourceId;
      public HealthMonitor(String sourceId, FabricClient fabricClient) {
         SourceId = sourceId;
         m_fabricClient = fabricClient;
      }
      public void ReportHealth(HealthReport healthReport) {
         if (healthReport != null)
            m_fabricClient.HealthManager.ReportHealth(healthReport); // Should be Async
      }

      #region Add methods
      public void Add(TimeSpan frequency, String property, Func<ClusterHealthCheckInfo, Task<ClusterHealthReport>> healthCheckAsync) =>
         Add(new ClusterHealthCheckInfo(this, frequency, property, healthCheckAsync));
      public void Add(TimeSpan frequency, String property, Func<PartitionHealthCheckInfo, Task<PartitionHealthReport>> healthCheckAsync) =>
         Add(new PartitionHealthCheckInfo(this, frequency, property, healthCheckAsync));
      public void Add(TimeSpan frequency, String property, Func<StatefulServiceReplicaHealthCheckInfo, Task<StatefulServiceReplicaHealthReport>> healthCheckAsync) =>
         Add(new StatefulServiceReplicaHealthCheckInfo(this, frequency, property, healthCheckAsync));
      public void Add(TimeSpan frequency, String property, Func<StatelessServiceInstanceHealthCheckInfo, Task<StatelessServiceInstanceHealthReport>> healthCheckAsync) =>
         Add(new StatelessServiceInstanceHealthCheckInfo(this, frequency, property, healthCheckAsync));
      public void Add(TimeSpan frequency, String property, Func<NodeHealthCheckInfo, Task<NodeHealthReport>> healthCheckAsync) =>
         Add(new NodeHealthCheckInfo(this, frequency, property, healthCheckAsync));
      public void Add(TimeSpan frequency, String property, Func<DeployedServicePackageHealthCheckInfo, Task<DeployedServicePackageHealthReport>> healthCheckAsync) =>
         Add(new DeployedServicePackageHealthCheckInfo(this, frequency, property, healthCheckAsync));
      public void Add(TimeSpan frequency, String property, Func<DeployedApplicationHealthCheckInfo, Task<DeployedApplicationHealthReport>> healthCheckAsync) =>
         Add(new DeployedApplicationHealthCheckInfo(this, frequency, property, healthCheckAsync));
      public void Add(TimeSpan frequency, String property, Func<ApplicationHealthCheckInfo, Task<ApplicationHealthReport>> healthCheckAsync) =>
         Add(new ApplicationHealthCheckInfo(this, frequency, property, healthCheckAsync));
      public void Add(TimeSpan frequency, String property, Func<ServiceHealthCheckInfo, Task<ServiceHealthReport>> healthCheckAsync) =>
         Add(new ServiceHealthCheckInfo(this, frequency, property, healthCheckAsync));
      #endregion
      private void Add(HealthCheckInfo hci) {
         if (!m_healthChecks.TryAdd(hci.ToString(), hci))
            throw new InvalidOperationException($"A health check with Kind={hci.Kind} and Property={hci.Property} was already registered.");
         hci.Start();
      }

      internal void Remove(HealthCheckInfo hci) => m_healthChecks.TryRemove(hci.ToString(), out hci);

      public IEnumerator<KeyValuePair<String, HealthCheckInfo>> GetEnumerator() => m_healthChecks.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => m_healthChecks.GetEnumerator();
   }

   public sealed class ClusterHealthCheckInfo : HealthCheckInfo {
      private readonly Func<ClusterHealthCheckInfo, Task<ClusterHealthReport>> HealthCheckAsync;
      internal ClusterHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<ClusterHealthCheckInfo, Task<ClusterHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.Cluster, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) 
         => await HealthCheckAsync((ClusterHealthCheckInfo)hci);
   }
   public sealed class PartitionHealthCheckInfo : HealthCheckInfo {
      private readonly Func<PartitionHealthCheckInfo, Task<PartitionHealthReport>> HealthCheckAsync;
      internal PartitionHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<PartitionHealthCheckInfo, Task<PartitionHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.Partition, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) => await HealthCheckAsync((PartitionHealthCheckInfo)hci);
   }
   public sealed class StatefulServiceReplicaHealthCheckInfo : HealthCheckInfo {
      private readonly Func<StatefulServiceReplicaHealthCheckInfo, Task<StatefulServiceReplicaHealthReport>> HealthCheckAsync;
      internal StatefulServiceReplicaHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<StatefulServiceReplicaHealthCheckInfo, Task<StatefulServiceReplicaHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.StatefulServiceReplica, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) 
         => await HealthCheckAsync((StatefulServiceReplicaHealthCheckInfo)hci);
   }
   public sealed class StatelessServiceInstanceHealthCheckInfo : HealthCheckInfo {
      private readonly Func<StatelessServiceInstanceHealthCheckInfo, Task<StatelessServiceInstanceHealthReport>> HealthCheckAsync;
      internal StatelessServiceInstanceHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<StatelessServiceInstanceHealthCheckInfo, Task<StatelessServiceInstanceHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.StatelessServiceInstance, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) 
         => await HealthCheckAsync((StatelessServiceInstanceHealthCheckInfo)hci);
   }
   public sealed class NodeHealthCheckInfo : HealthCheckInfo {
      private readonly Func<NodeHealthCheckInfo, Task<NodeHealthReport>> HealthCheckAsync;
      internal NodeHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<NodeHealthCheckInfo, Task<NodeHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.Node, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) 
         => await HealthCheckAsync((NodeHealthCheckInfo)hci);
   }
   public sealed class ServiceHealthCheckInfo : HealthCheckInfo {
      private readonly Func<ServiceHealthCheckInfo, Task<ServiceHealthReport>> HealthCheckAsync;
      internal ServiceHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<ServiceHealthCheckInfo, Task<ServiceHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.Service, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) 
         => await HealthCheckAsync((ServiceHealthCheckInfo)hci);
   }
   public sealed class ApplicationHealthCheckInfo : HealthCheckInfo {
      private readonly Func<ApplicationHealthCheckInfo, Task<ApplicationHealthReport>> HealthCheckAsync;
      internal ApplicationHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<ApplicationHealthCheckInfo, Task<ApplicationHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.Application, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) 
         => await HealthCheckAsync((ApplicationHealthCheckInfo)hci);
   }
   public sealed class DeployedApplicationHealthCheckInfo : HealthCheckInfo {
      private readonly Func<DeployedApplicationHealthCheckInfo, Task<DeployedApplicationHealthReport>> HealthCheckAsync;
      internal DeployedApplicationHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<DeployedApplicationHealthCheckInfo, Task<DeployedApplicationHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.DeployedApplication, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) 
         => await HealthCheckAsync((DeployedApplicationHealthCheckInfo)hci);
   }
   public sealed class DeployedServicePackageHealthCheckInfo : HealthCheckInfo {
      private readonly Func<DeployedServicePackageHealthCheckInfo, Task<DeployedServicePackageHealthReport>> HealthCheckAsync;
      internal DeployedServicePackageHealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency,
         String property, Func<DeployedServicePackageHealthCheckInfo, Task<DeployedServicePackageHealthReport>> healthCheckAsync)
         : base(healthMonitor, frequency, HealthReportKind.DeployedServicePackage, property) {
         HealthCheckAsync = healthCheckAsync;
      }
      protected override async Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci) 
         => await HealthCheckAsync((DeployedServicePackageHealthCheckInfo)hci);
   }
   public abstract class HealthCheckInfo {
      internal readonly HealthMonitor HealthMonitor;
      internal readonly Timer Timer;
      public readonly HealthReportKind Kind;
      public readonly String Property;
      public TimeSpan Frequency;

      public String SourceId => HealthMonitor.SourceId;

      protected internal HealthCheckInfo(HealthMonitor healthMonitor, TimeSpan frequency, HealthReportKind kind, String property) {
         HealthMonitor = healthMonitor;
         Frequency = frequency;
         Kind = kind;
         Property = property;
         Timer = new Timer(o => DoHealthCheckAsync(), null, Timeout.Infinite, Timeout.Infinite);
      }
      internal void Start() => Timer.Change(Frequency, Timeout.InfiniteTimeSpan);

      private async void DoHealthCheckAsync() {
         try {
            var healthReport = await OnHealthCheckAsync(this);
            HealthMonitor.ReportHealth(healthReport);
         }
         finally {
            Timer.Change(Frequency, Timeout.InfiniteTimeSpan);
         }
      }
      protected abstract Task<HealthReport> OnHealthCheckAsync(HealthCheckInfo hci);
      public override string ToString() => $"{Kind}_{Property}";

      public HealthInformation ToHealthInformation(HealthState state, TimeSpan ttl, Boolean removedWhenExpired = false,
         String description = null, Int64 sequenceNumber = HealthInformation.AutoSequenceNumber) {
         return new HealthInformation(SourceId, Property, state) {
            TimeToLive = ttl, RemoveWhenExpired = removedWhenExpired,
            Description = description,
            SequenceNumber = sequenceNumber
         };
      }
      public void Remove() {
         Timer.Dispose();
         HealthMonitor.Remove(this);
      }
   }
}
