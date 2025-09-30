using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace AgonesSdkCsharp.Hosting;

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
    /// Allowed failed count before trigger circuit break
    /// </summary>
    public int HandledEventsAllowedBeforeCirtcuitBreaking { get; set; } = 5;
    /// <summary>
    /// Circuit break Duration
    /// </summary>
    public TimeSpan CirtcuitBreakingDuration { get; set; } = TimeSpan.FromSeconds(5);
    /// <summary>
    /// Action on Circuit Break
    /// </summary>
    public Func<DelegateResult<HttpResponseMessage>, CircuitState, TimeSpan, Context, ILogger<AgonesCircuitDelegate>, Task> OnBreak { get; set; } = AgonesCircuitDelegate.OnBreakDefault;
    /// <summary>
    /// Action on Circuit Break Reset
    /// </summary>
    public Action<Context, ILogger<AgonesCircuitDelegate>> OnReset { get; set; } = AgonesCircuitDelegate.OnResetDefault;
    /// <summary>
    /// Action on Circui Break HalfOpen
    /// </summary>
    public Action<ILogger<AgonesCircuitDelegate>> OnHalfOpen { get; set; } = AgonesCircuitDelegate.OnHalfOpenDefault;
}
