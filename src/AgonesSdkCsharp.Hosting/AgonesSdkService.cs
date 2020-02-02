using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AgonesSdkCsharp.Hosting
{
    public static class AgonesSdkService
    {
        private static readonly Lazy<Random> jitterer = new Lazy<Random>(() => new Random());

        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(this IHostBuilder hostBuilder) where T : class, IAgonesSdk
            => hostBuilder.UseAgones<T>(configureSdk => { }, configureService => { });
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureSdk"></param>
        /// <param name="configureHosting"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(this IHostBuilder hostBuilder, Action<AgonesSdkOptions> configureSdk) where T : class, IAgonesSdk
            => hostBuilder.UseAgones<T>(configureSdk, hosting => { });
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureHosting"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(this IHostBuilder hostBuilder, Action<AgonesSdkHostingOptions> configureHosting) where T : class, IAgonesSdk
            => hostBuilder.UseAgones<T>(sdk => { }, configureHosting);
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureSdk"></param>
        /// <param name="configureHosting"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(this IHostBuilder hostBuilder,  Action<AgonesSdkOptions> configureSdk, Action<AgonesSdkHostingOptions> configureHosting) where T : class, IAgonesSdk
        {
            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                var sdkOptions = new AgonesSdkOptions();
                configureSdk.Invoke(sdkOptions);

                var serviceOptions = new AgonesSdkHostingOptions();
                configureHosting.Invoke(serviceOptions);

                if (serviceOptions.UseDefaultHttpClientFactory)
                {
                    var sp = services.BuildServiceProvider();
                    var loggerFactory = sp.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<AgonesHealthCheckService>();

                    var httpClientBuilder = services.AddHttpClient(sdkOptions.HttpClientName, client =>
                    {
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("User-Agent", sdkOptions.HttpClientUserAgent);
                    })
                    .AddTransientHttpErrorPolicy(x => x.WaitAndRetryAsync(serviceOptions.FailedRetryCount, 
                        sleepDurationProvider: retry => ExponentialBackkoff(retry), 
                        onRetry: (response, duration, context) => serviceOptions.OnRetry(response, duration, context, logger)))
                    .AddTransientHttpErrorPolicy(x => x.CircuitBreakerAsync(
                        serviceOptions.HandledEventsAllowedBeforeCirtcuitBreaking,
                        serviceOptions.CirtcuitBreakingDuration,
                        onBreak: (response, state, duration, context) => serviceOptions.OnBreak(response, state, duration, context, logger),
                        onReset: (Context context) => serviceOptions.OnReset(context, logger),
                        onHalfOpen: () => serviceOptions.OnHalfOpen(logger))
                    );
                }

                services.AddSingleton<AgonesSdkOptions>(sdkOptions);
                services.AddSingleton<IAgonesSdk, T>();
                if (serviceOptions.RegisterHealthCheckService)
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
