using Common;
using System;
using System.Diagnostics;
using System.Fabric;
using System.Fabric.Health;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MonitorService
{
    /*
     * Credit to Jeff Richter - Azure Service Fabric Team 
     * This is a Service-Fabric-aware console app that monitors another service and reports health information about it
     */
    static class Program
    {
        private static int _port = 0;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    throw new Exception("An http port # is needed to be passed in the argument");

                _port = Int32.Parse(args[0]);

                // https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-diagnostics-how-to-report-and-check-service-health
                // Works on non-secure cluster
                FabricClient fc = new FabricClient(new FabricClientSettings { HealthReportSendInterval = TimeSpan.FromSeconds(0) });

                // Delay a bit before starting to monitor
                Task.Delay(5000).Wait();

                var hm = new HealthMonitor(Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]), fc)
                {
                    { TimeSpan.FromSeconds(10), "PerformanceCheck", PerformanceCheckAsync },
                    //{ TimeSpan.FromSeconds(15), "ClusterCheck", ClusterCheck }
                };

                Console.ReadLine();

                foreach (var h in hm)
                    h.Value.Remove();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Monitor service failed: {ex.Message} - Inner: {ex.InnerException}");
                throw ex;
            }

            Console.ReadLine();
        }

        private static async Task<ServiceHealthReport> PerformanceCheckAsync(ServiceHealthCheckInfo hci)
        {
            HealthState healthState = HealthState.Ok;
            var sw = Stopwatch.StartNew();
            try
            {
                var url = Constants.GetCrashableWebServiceBaseUrl(_port);
                // The HttpClient does not know how to parse the '+' ...so we must use a real host name
                url = url.Replace("+", "localhost"); // TODO: Change to an argument
                Console.WriteLine($"{DateTime.Now} - PerformanceCheckAsync - Checking performance on {url}");
                await new HttpClient().GetStringAsync(url);
                healthState = (sw.Elapsed < TimeSpan.FromMilliseconds(1000)) ? HealthState.Ok : HealthState.Warning;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now} - PerformanceCheckAsync failed: {e.Message} - Inner: {e.InnerException}");
                healthState = HealthState.Error;
            }

            String[] descriptions = new[] 
            {
                "Web server is responding well.",
                "Web server is responding slowly.",
                "Web server is not responding."
            };

            return new ServiceHealthReport(new Uri($"fabric:/{Constants.CrashableAppName}/{Constants.CrashableServiceName}"),
               hci.ToHealthInformation(healthState, TimeSpan.FromSeconds(15), false,
               descriptions[(Int32)healthState - 1]));
        }

        private static ClusterHealthReport ClusterCheck(ClusterHealthCheckInfo hci)
        {
            // TODO: Do the check
            return new ClusterHealthReport(hci.ToHealthInformation(HealthState.Error, TimeSpan.FromSeconds(100)));
        }
    }
}
