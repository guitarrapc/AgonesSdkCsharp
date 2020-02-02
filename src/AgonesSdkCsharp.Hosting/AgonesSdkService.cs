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
            => hostBuilder.UseAgones<T>(configureSdk => { }, configureService => { }, OnRetryDefault, OnBreakDefault, OnResetDefault, OnHalfOpenDefault);
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureSdk"></param>
        /// <param name="configureHosting"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(this IHostBuilder hostBuilder, Action<AgonesSdkOptions> configureSdk) where T : class, IAgonesSdk
            => hostBuilder.UseAgones<T>(configureSdk, hosting => { }, OnRetryDefault, OnBreakDefault, OnResetDefault, OnHalfOpenDefault);
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureHosting"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(this IHostBuilder hostBuilder, Action<AgonesSdkHostingOptions> configureHosting) where T : class, IAgonesSdk
            => hostBuilder.UseAgones<T>(sdk => { }, configureHosting, OnRetryDefault, OnBreakDefault, OnResetDefault, OnHalfOpenDefault);
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureSdk"></param>
        /// <param name="configureHosting"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(this IHostBuilder hostBuilder, Action<AgonesSdkOptions> configureSdk, Action<AgonesSdkHostingOptions> configureHosting) where T: class, IAgonesSdk
            => hostBuilder.UseAgones<T>(configureSdk, configureHosting, OnRetryDefault, OnBreakDefault, OnResetDefault, OnHalfOpenDefault);
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureSdk"></param>
        /// <param name="onRetry"></param>
        /// <param name="onBreak"></param>
        /// <param name="onReset"></param>
        /// <param name="onHalfOpen"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(
            this IHostBuilder hostBuilder, 
            Action<AgonesSdkOptions> configureSdk,
            Action<AgonesSdkHostingOptions> configureHosting,
            Func<DelegateResult<HttpResponseMessage>, TimeSpan, Context, ILogger<AgonesHealthCheckService>, Task> onRetry,
            Action<DelegateResult<HttpResponseMessage>, CircuitState, TimeSpan, Context, ILogger<AgonesHealthCheckService>> onBreak,
            Action<Context, ILogger<AgonesHealthCheckService>> onReset,
            Action<ILogger<AgonesHealthCheckService>> onHalfOpen
        ) where T : class, IAgonesSdk
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
                        onRetry: (response, duration, context) => onRetry(response, duration, context, logger)))
                    .AddTransientHttpErrorPolicy(x => x.CircuitBreakerAsync(
                        serviceOptions.HandledEventsAllowedBeforeCirtcuitBreaking,
                        serviceOptions.CirtcuitBreakingDuration,
                        onBreak: (response, state, duration, context) => onBreak(response, state, duration, context, logger),
                        onReset: (Context context) => onReset(context, logger),
                        onHalfOpen: () => onHalfOpen(logger))
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

        private static Task OnRetryDefault(DelegateResult<HttpResponseMessage> response, TimeSpan duration, Context context, ILogger<AgonesHealthCheckService> logger)
        {
            if (response.Result == null)
            {
                logger?.LogError($"OnRetry: Failed connection. CorrelationId {context.CorrelationId}; Exception {response.Exception?.Message}");
            }
            else
            {
                logger?.LogInformation($"OnRetry: Failed connection. CorrelationId {context.CorrelationId}; StatusCode {response.Result?.StatusCode}; Reason {response.Result?.ReasonPhrase}; Exception {response.Exception?.Message}");
            }
            return Task.FromResult(0);
        }
        private static void OnBreakDefault(DelegateResult<HttpResponseMessage> response, CircuitState state, TimeSpan duration, Context context, ILogger<AgonesHealthCheckService> logger)
        {
            if (response.Result == null)
            {
                logger?.LogError($"OnBreak: Circuit cut, requests will not flow. CorrelationId {context.CorrelationId}; Exception {response.Exception?.Message}");
            }
            else
            {
                logger?.LogInformation($"OnBreak: Circuit cut, requests will not flow. CorrelationId {context.CorrelationId}; StatusCode {response.Result?.StatusCode}; Reason {response.Result?.ReasonPhrase}; Exception {response.Exception?.Message}");
            }
        }
        private static void OnResetDefault(Context context, ILogger<AgonesHealthCheckService> logger)
        {
            logger?.LogInformation($"OnReset: Circuit closed, requests flow normally. CorrelationId {context.CorrelationId}");
        }
        private static void OnHalfOpenDefault(ILogger<AgonesHealthCheckService> logger)
        {
            logger.LogInformation("OnHalfOpen: Circuit in test mode, one request will be allowed.");
        }
    }
}
