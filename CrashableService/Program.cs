using System;
using System.Threading;

namespace CrashableService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Crashable Service Started....");

            while(true)
            {
                Thread.Sleep(120000);
            }
            //Console.WriteLine("Crashable Service crashed...");
            // Crash!!!!!
            //Environment.Exit(-1);
        }
    }
}
