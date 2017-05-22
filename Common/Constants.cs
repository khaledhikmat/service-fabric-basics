namespace Common
{
    public class Constants
    {
        // Service Fabric specific
        public const string CrashableAppName = "BasicAvailabilityApp";
        public const string CrashableServiceName = "CrashableService";

        public static string GetCrashableWebServiceBaseUrl(int port)
        {
            // The + is needed to allow the HttpListener to match the host name
            // https://github.com/aspnet/Hosting/issues/749
            return $"http://+:{port}";
        }
    }
}
