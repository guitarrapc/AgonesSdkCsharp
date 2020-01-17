using System;

namespace AgonesSdk
{
    /// <summary>
    /// AgonesSdk Settings
    /// </summary>
    public class AgonesSdkSettings
    {
        /// <summary>
        /// HttpClientName for AgonesSdk
        /// </summary>
        public string HttpClientName { get; set; } = "Agones";
        /// <summary>
        /// Determine cache request
        /// </summary>
        public bool CacheRequest { get; set; } = true;
        /// <summary>
        /// Health Check Interval
        /// </summary>
        public TimeSpan HealthInterval { get; set; } = TimeSpan.FromSeconds(2);
        /// <summary>
        /// AgonesSdk Polly Settings
        /// </summary>
        public AgonesSdkHttpPollySettings PollySettings { get; set; } = new AgonesSdkHttpPollySettings();
    }

    /// <summary>
    /// AgonesSdk Http Polly Settings
    /// </summary>
    public class AgonesSdkHttpPollySettings
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
