using AgonesSdk.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .AddAgonesSdk()
            .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug));
    }
}
