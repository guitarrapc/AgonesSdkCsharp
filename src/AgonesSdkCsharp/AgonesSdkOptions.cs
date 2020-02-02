using System;
using System.Text.Json;

namespace AgonesSdkCsharp
{
    /// <summary>
    /// AgonesSdk Options
    /// </summary>
    public class AgonesSdkOptions
    {
        public const string DefaultHttpClientName = "Agones";

        public bool UseDefaultHttpClientFactory { get; set; } = true;
        public bool RegisterHealthCheckService { get; set; } = true;
        /// <summary>
        /// HttpClientName for AgonesSdk
        /// </summary>
        public string HttpClientName { get; set; } = DefaultHttpClientName;
        /// <summary>
        /// HttpClientName for AgonesSdk
        /// </summary>
        public string HttpClientUserAgent { get; set; } = "AgonesSdkCsharp";
        /// <summary>
        /// Determine cache request
        /// </summary>
        public bool CacheRequest { get; set; } = true;
        /// <summary>
        /// Health Check Interval
        /// </summary>
        public TimeSpan HealthInterval { get; set; } = TimeSpan.FromSeconds(2);
        /// <summary>
        /// AgonesSdk Polly Options
        /// </summary>
        public AgonesSdkHttpPollyOptions PollyOptions { get; set; } = new AgonesSdkHttpPollyOptions();

        public string AsJson()
        {
            var json = JsonSerializer.Serialize<AgonesSdkOptions>(this);
            return json;
        }
    }

    /// <summary>
    /// AgonesSdk Http Polly Options
    /// </summary>
    public class AgonesSdkHttpPollyOptions
    {
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
    }
}
