using System.Text.Json.Serialization;

namespace AgonesSdkCsharp
{
    public class NullResponse { }

    public class GameServerResponse
    {
        [JsonPropertyName("object/meta")]
        public ObjectMeta ObjectMeta { get; set; }
        [JsonPropertyName("status")]
        public Status Status { get; set; }
    }

    public class ObjectMeta
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("namespace")]
        public string Namespace { get; set; }
        [JsonPropertyName("uid")]
        public string Uid { get; set; }
        [JsonPropertyName("resource/version")]
        public string ResourceVersion { get; set; }
        [JsonPropertyName("generation")]
        public string Generation { get; set; }
        [JsonPropertyName("creation/timestamp")]
        public string CreationTimestamp { get; set; }
        [JsonPropertyName("annotations")]
        public Annotation[] Annotations { get; set; }
        [JsonPropertyName("labels")]
        public Label[] Labels { get; set; }
    }

    public class Annotation
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Label
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Status
    {
        [JsonPropertyName("state")]
        public string State { get; set; }
        [JsonPropertyName("address")]
        public string Address { get; set; }
        [JsonPropertyName("ports")]
        public PortInfo[] Ports { get; set; }
    }

    public class PortInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("port")]
        public int Port { get; set; }
    }
}
