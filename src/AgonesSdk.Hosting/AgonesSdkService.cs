using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System;

namespace AgonesSdk.Hosting
{
    public static class AgonesSdkService
    {
        private static readonly Lazy<Random> jitterer = new Lazy<Random>(() => new Random());

        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T">Type implements <see cref="IAgonesSdk"/></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="useDefaultHttpClientFactory">set false when you register your own <see cref="IHttpClientFactory"/></param>
        /// <param name="registerHealthCheckService">register Background Healthcheck service</param>
        /// <returns></returns>
        public static IHostBuilder AddAgones<T>(this IHostBuilder hostBuilder, bool useDefaultHttpClientFactory = true, bool registerHealthCheckService = true) where T : class, IAgonesSdk
            => hostBuilder.AddAgones<T>(new AgonesSdkOptions(), useDefaultHttpClientFactory, registerHealthCheckService);
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T">Type implements <see cref="IAgonesSdk"/></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="settings"></param>
        /// <param name="useDefaultHttpClientFactory">set false when you register your own <see cref="IHttpClientFactory"/></param>
        /// <param name="registerHealthCheckService">register Background Healthcheck service</param>
        /// <returns></returns>
        public static IHostBuilder AddAgones<T>(this IHostBuilder hostBuilder, AgonesSdkOptions settings, bool useDefaultHttpClientFactory = true, bool registerHealthCheckService = true) where T: class, IAgonesSdk
        {
            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                if (!useDefaultHttpClientFactory)
                {
                    services.AddHttpClient(settings.HttpClientName, client =>
                    {
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("User-Agent", settings.HttpClientUserAgent);
                    })
                    .AddTransientHttpErrorPolicy(x => x.WaitAndRetryAsync(settings.PollyOptions.FailedRetryCount, retry => ExponentialBackkoff(retry)))
                    .AddTransientHttpErrorPolicy(x => x.CircuitBreakerAsync(settings.PollyOptions.HandledEventsAllowedBeforeCirtcuitBreaking, settings.PollyOptions.CirtcuitBreakingDuration));
                }

                services.AddSingleton<AgonesSdkOptions>(settings);
                services.AddSingleton<IAgonesSdk, T>();

                if (registerHealthCheckService)
                {
                    services.AddHostedService<AgonesHealthCheckService>();
                }
            });
        }

        /// <summary>
        /// Provide Exponential Backoff
        /// </summary>
        /// <param name="retryAttempt"></param>
        /// <returns></returns>
        public static TimeSpan ExponentialBackkoff(int retryAttempt)
            => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Value.Next(0, 100));
    }
}
