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
            => hostBuilder.UseAgones<T>(op => { }, OnRetryDefault, OnBreakDefault, OnResetDefault, OnHalfOpenDefault);
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T">Type implements <see cref="IAgonesSdk"/></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="settings"></param>
        /// <param name="useDefaultHttpClientFactory">set false when you register your own <see cref="IHttpClientFactory"/></param>
        /// <param name="registerHealthCheckService">register Background Healthcheck service</param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(this IHostBuilder hostBuilder, Action<AgonesSdkOptions> options) where T: class, IAgonesSdk
            => hostBuilder.UseAgones<T>(options, OnRetryDefault, OnBreakDefault, OnResetDefault, OnHalfOpenDefault);
        /// <summary>
        /// Add Agones Sdk and run Health Check in the background.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="configureOptions"></param>
        /// <param name="onRetry"></param>
        /// <param name="onBreak"></param>
        /// <param name="onReset"></param>
        /// <param name="onHalfOpen"></param>
        /// <returns></returns>
        public static IHostBuilder UseAgones<T>(
            this IHostBuilder hostBuilder, 
            Action<AgonesSdkOptions> configureOptions,
            Func<DelegateResult<HttpResponseMessage>, TimeSpan, Context, ILogger<AgonesHealthCheckService>, Task> onRetry,
            Action<DelegateResult<HttpResponseMessage>, CircuitState, TimeSpan, Context, ILogger<AgonesHealthCheckService>> onBreak,
            Action<Context, ILogger<AgonesHealthCheckService>> onReset,
            Action<ILogger<AgonesHealthCheckService>> onHalfOpen
        ) where T : class, IAgonesSdk
        {
            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                var options = new AgonesSdkOptions();
                configureOptions.Invoke(options);

                if (options.UseDefaultHttpClientFactory)
                {
                    var sp = services.BuildServiceProvider();
                    var loggerFactory = sp.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<AgonesHealthCheckService>();

                    var httpClientBuilder = services.AddHttpClient(options.HttpClientName, client =>
                    {
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("User-Agent", options.HttpClientUserAgent);
                    })
                    .AddTransientHttpErrorPolicy(x => x.WaitAndRetryAsync(options.PollyOptions.FailedRetryCount, 
                        sleepDurationProvider: retry => ExponentialBackkoff(retry), 
                        onRetry: (response, duration, context) => onRetry(response, duration, context, logger)))
                    .AddTransientHttpErrorPolicy(x => x.CircuitBreakerAsync(
                        options.PollyOptions.HandledEventsAllowedBeforeCirtcuitBreaking,
                        options.PollyOptions.CirtcuitBreakingDuration,
                        onBreak: (response, state, duration, context) => onBreak(response, state, duration, context, logger),
                        onReset: (Context context) => onReset(context, logger),
                        onHalfOpen: () => onHalfOpen(logger))
                    );
                }

                services.AddSingleton<AgonesSdkOptions>(options);
                services.AddSingleton<IAgonesSdk, T>();
                if (options.RegisterHealthCheckService)
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
