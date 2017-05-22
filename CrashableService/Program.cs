using Common;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CrashableService
{
    /*
     * 
     * https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server
     * Slightltly modified to allow await/async
     *
     * This is a non-service-Fabric-aware web service deployed in Service Fabric as guest executable
     * VS MUST in ADMININSTRATOR mode to test locally!!!!
     */
    class Program
    {
        private static int _delay = 0;
        private static string _node = Environment.GetEnvironmentVariable("HostedServiceName") ?? "Not running on Service Fabric";

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    throw new Exception("An http port # is needed to be passed in the argument");

                int port = Int32.Parse(args[0]);
                WebServer ws = new WebServer(ProcessRequest, $"{Constants.GetCrashableWebServiceBaseUrl(port)}/");
                ws.Run();
                Console.WriteLine("A simple crashable webserver. Press a key to quit.");
                Console.ReadKey();
                ws.Stop();

            }
            catch (Exception e)
            {
                Console.WriteLine($"Crashable serice failed: {e.Message} - Inner: {e.InnerException}");
            }
        }

        // The command names are credit to Jeff Richter of the MSFT ServiceFabric Team
        public static async Task<string> ProcessRequest(HttpListenerRequest request)
        {
            var command = request.QueryString["cmd"];
            if (!string.IsNullOrEmpty(command))
            {
                switch (command.ToLowerInvariant())
                {
                    case "delay":
                        Int32.TryParse(request.QueryString["delay"], out _delay);
                        break;
                    case "crash":
                        Environment.Exit(-1);// throw new InvalidOperationException("Forced crash");
                        break;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(_delay));
            return $"<HTML><BODY><strong>Node:</strong> {_node} - <strong>Delay:</strong> {_delay} - <strong>Date:</strong> {DateTime.Now} - <strong>Command:</strong> {command}</BODY></HTML>";
        }
    }
}
