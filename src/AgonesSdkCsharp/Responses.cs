using System.Text.Json.Serialization;

namespace AgonesSdkCsharp
{
    public class NullResponse { }

    public class GameServerResponse
    {
        [JsonPropertyName("object/meta")]
        public Object_Meta object_meta { get; set; }
        [JsonPropertyName("status")]
        public Status status { get; set; }

        public class Object_Meta
        {
            [JsonPropertyName("name")]
            public string name { get; set; }
            [JsonPropertyName("namespace")]
            public string @namespace { get; set; }
            [JsonPropertyName("uid")]
            public string uid { get; set; }
            [JsonPropertyName("resource/version")]
            public string resource_version { get; set; }
            [JsonPropertyName("generation")]
            public string generation { get; set; }
            [JsonPropertyName("creation/timestamp")]
            public string creation_timestamp { get; set; }
            [JsonPropertyName("annotations")]
            public Annotations annotations { get; set; }
            [JsonPropertyName("labels")]
            public Labels labels { get; set; }
        }

        public class Annotations
        {
            [JsonPropertyName("annotation")]
            public string annotation { get; set; }
        }

        public class Labels
        {
            [JsonPropertyName("islocal")]
            public string islocal { get; set; }
        }

        public class Status
        {
            [JsonPropertyName("state")]
            public string state { get; set; }
            [JsonPropertyName("address")]
            public string address { get; set; }
            [JsonPropertyName("ports")]
            public Port[] ports { get; set; }
        }

        public class Port
        {
            [JsonPropertyName("name")]
            public string name { get; set; }
            [JsonPropertyName("port")]
            public int port { get; set; }
        }
    }
}
