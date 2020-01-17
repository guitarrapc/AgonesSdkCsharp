using AgonesSdk.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace SampleHosting
{
    class Program
    {
        static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            //CreateHostBuilderAgonesSettings(args).Build().Run();
            CreateHostBuilderHttpService(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .AddAgones()
            .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)); // HealtchCheckService Log

        public static IHostBuilder CreateHostBuilderAgonesSettings(string[] args)
        {
            var settings = new AgonesSdk.AgonesSdkOptions
            {
                HealthInterval = TimeSpan.FromSeconds(1),
                HttpClientName = "myAgonesClient",
                PollyOptions = new AgonesSdk.AgonesSdkHttpPollyOptions
                {
                    FailedRetryCount = 5,
                    CirtcuitBreakingDuration = TimeSpan.FromSeconds(10),
                    HandledEventsAllowedBeforeCirtcuitBreaking = 2,
                },
            };
            return Host.CreateDefaultBuilder(args)
                .AddAgones(settings)
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)); // HealtchCheckService Log
        }

        /// <summary>
        /// Use your HttpClient for AgonesSdk
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilderHttpService(string[] args)
        {
            var settings = new AgonesSdk.AgonesSdkOptions();

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // set HttpClientName pass with settings
                    services.AddHttpClient(settings.HttpClientName, client =>
                    {
                        // you must set at least RequesetHeader. (MUST BE application/json)
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    });
                })
                .AddAgones(settings, useDefaultHttpClientFactory: false)
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)); // HealtchCheckService Log
        }
    }
}
