using System.Text.Json;

namespace AgonesSdkCsharp;

/// <summary>
/// AgonesSdk Options
/// </summary>
public class AgonesSdkOptions
{
    public const string DefaultHttpClientName = "Agones";
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

    public string AsJson()
    {
        var json = JsonSerializer.Serialize<AgonesSdkOptions>(this);
        return json;
    }
}
