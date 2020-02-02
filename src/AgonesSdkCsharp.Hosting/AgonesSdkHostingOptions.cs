using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AgonesSdkCsharp.Hosting
{
    /// <summary>
    /// AgonesSdk Hosting Options
    /// </summary>
    public class AgonesSdkHostingOptions
    {
        /// <summary>
        /// Use Hosting Integrated HttpClientFactory. false to use your own HttpClientFactory.
        /// </summary>
        public bool UseDefaultHttpClientFactory { get; set; } = true;
        /// <summary>
        /// Register Health Check background service. false to unregister background service.
        /// </summary>
        public bool RegisterHealthCheckService { get; set; } = true;
        /// <summary>
        /// Failed count when to trigger wait and retry.
        /// </summary>
        public int FailedRetryCount { get; set; } = 3;
        /// <summary>
        /// Allowed failed count before trigger circuit break
        /// </summary>
        public int HandledEventsAllowedBeforeCirtcuitBreaking { get; set; } = 5;
        /// <summary>
        /// Circuit break Duration
        /// </summary>
        public TimeSpan CirtcuitBreakingDuration { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// Action on Polly Retry
        /// </summary>
        public Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context, ILogger<AgonesHealthCheckService>> OnRetry { get; set; } = OnRetryDefault;
        /// <summary>
        /// Action on Circuit Break
        /// </summary>
        public Action<DelegateResult<HttpResponseMessage>, CircuitState, TimeSpan, Context, ILogger<AgonesHealthCheckService>> OnBreak { get; set; } = OnBreakDefault;
        /// <summary>
        /// Action on Circuit Break Reset
        /// </summary>
        public Action<Context, ILogger<AgonesHealthCheckService>> OnReset { get; set; } = OnResetDefault;
        /// <summary>
        /// Action on Circui Break HalfOpen
        /// </summary>
        public Action<ILogger<AgonesHealthCheckService>> OnHalfOpen { get; set; } = OnHalfOpenDefault;

        private static void OnRetryDefault(DelegateResult<HttpResponseMessage> response, TimeSpan duration, Context context, ILogger<AgonesHealthCheckService> logger)
        {
            if (response.Result == null)
            {
                logger?.LogError($"OnRetry: Failed connection. CorrelationId {context.CorrelationId}; Exception {response.Exception?.Message}");
            }
            else
            {
                logger?.LogInformation($"OnRetry: Failed connection. CorrelationId {context.CorrelationId}; StatusCode {response.Result?.StatusCode}; Reason {response.Result?.ReasonPhrase}; Exception {response.Exception?.Message}");
            }
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
