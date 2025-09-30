using System.Text.Json.Serialization;

namespace AgonesSdkCsharp;

public class NullResponse { }

public class GameServerResponse
{
    public static GameServerResponse Empty => new GameServerResponse
    {
        ObjectMeta = new ObjectMeta
        {
            Name = string.Empty,
            Namespace = string.Empty,
            Uid = string.Empty,
            ResourceVersion = string.Empty,
            Generation = string.Empty,
            CreationTimestamp = string.Empty,
            Annotations = [],
            Labels = [],
        },
        Status = new Status
        {
            State = string.Empty,
            Address = string.Empty,
            Ports = [],
        }
    };

    [JsonPropertyName("object/meta")]
    public required ObjectMeta ObjectMeta { get; set; }
    [JsonPropertyName("status")]
    public required Status Status { get; set; }
}

public class ObjectMeta
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("namespace")]
    public required string Namespace { get; set; }
    [JsonPropertyName("uid")]
    public required string Uid { get; set; }
    [JsonPropertyName("resource/version")]
    public required string ResourceVersion { get; set; }
    [JsonPropertyName("generation")]
    public required string Generation { get; set; }
    [JsonPropertyName("creation/timestamp")]
    public required string CreationTimestamp { get; set; }
    [JsonPropertyName("annotations")]
    public required Annotation[] Annotations { get; set; }
    [JsonPropertyName("labels")]
    public required Label[] Labels { get; set; }
}

public class Annotation
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}

public class Label
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}

public class Status
{
    [JsonPropertyName("state")]
    public required string State { get; set; }
    [JsonPropertyName("address")]
    public required string Address { get; set; }
    [JsonPropertyName("ports")]
    public required PortInfo[] Ports { get; set; }
}

public class PortInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("port")]
    public required int Port { get; set; }
}
