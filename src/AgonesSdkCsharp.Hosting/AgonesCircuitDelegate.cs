using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AgonesSdkCsharp.Hosting
{
    public class AgonesCircuitDelegate
    {
        public static Task OnBreakDefault(DelegateResult<HttpResponseMessage> response, CircuitState state, TimeSpan duration, Context context, ILogger<AgonesCircuitDelegate> logger)
        {
            if (response.Result == null)
            {
                logger?.LogDebug($"OnBreak: Circuit cut, requests will not flow. State {state}; CorrelationId {context.CorrelationId}; Exception {response.Exception?.Message}");
            }
            else
            {
                if (response.Exception == null)
                {
                    logger?.LogDebug($"OnBreak: Circuit cut, requests will not flow. State {state}; CorrelationId {context.CorrelationId}; StatusCode {response.Result?.StatusCode}; Reason {response.Result?.ReasonPhrase};");
                }
                else
                {
                    logger?.LogDebug($"OnBreak: Circuit cut, requests will not flow. State {state}; CorrelationId {context.CorrelationId}; StatusCode {response.Result?.StatusCode}; Reason {response.Result?.ReasonPhrase}; Exception {response.Exception.Message}");
                }
            }
            return Task.Delay(duration);
        }
        public static void OnResetDefault(Context context, ILogger<AgonesCircuitDelegate> logger)
        {
            logger?.LogDebug($"OnReset: Circuit closed, requests flow normally. CorrelationId {context.CorrelationId}");
        }
        public static void OnHalfOpenDefault(ILogger<AgonesCircuitDelegate> logger)
        {
            logger.LogDebug("OnHalfOpen: Circuit in test mode, one request will be allowed.");
        }
    }

}
