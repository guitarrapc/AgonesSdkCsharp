using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AgonesSdk
{
    // ref: sdk sample https://github.com/googleforgames/agones/blob/release-1.2.0/sdks/go/sdk.go
    public class AgonesSdk : IAgonesSdk
    {
        static readonly Encoding encoding = new UTF8Encoding(false);
        static readonly Lazy<ConcurrentDictionary<string, StringContent>> jsonCache = new Lazy<ConcurrentDictionary<string, StringContent>>(() => new ConcurrentDictionary<string, StringContent>());

        public bool HealthEnabled { get; set; } = true;

        // ref: sdk server https://github.com/googleforgames/agones/blob/master/cmd/sdk-server/main.go
        // grpc: localhost on port 9357
        // http: localhost on port 9358
        readonly Uri SideCarAddress = new Uri("http://127.0.0.1:9358");
        readonly AgonesSdkSettings _settings;
        readonly IHttpClientFactory _httpClientFactory;

        public AgonesSdk(AgonesSdkSettings settings, IHttpClientFactory httpClientFactory)
        {
            _settings = settings;
            _httpClientFactory = httpClientFactory;

            if (_settings.CacheRequest)
            {
                // cache empty request content
                var stringContent = new StringContent("{}", encoding, "application/json");
                stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                jsonCache.Value.TryAdd("{}", stringContent);
            }
        }

        public Task Ready(CancellationToken ct = default)
        {            
            return SendRequestAsync<NullResponse>("/ready", "{}", ct);
        }

        public Task Allocate(CancellationToken ct = default)
        {            
            return SendRequestAsync<NullResponse>("/allocate", "{}", ct);
        }

        public Task Shutdown(CancellationToken ct = default)
        {            
            return SendRequestAsync<NullResponse>("/shutdown", "{}", ct);
        }

        public Task Health(CancellationToken ct = default)
        {            
            return SendRequestAsync<NullResponse>("/health", "{}", ct);
        }

        public Task<GameServerResponse> GameServer(CancellationToken ct = default)
        {
            return SendRequestAsync<GameServerResponse>("/gameserver", "{}", HttpMethod.Get, ct);
        }

        public Task<GameServerResponse> Watch(CancellationToken ct = default)
        {            
            return SendRequestAsync<GameServerResponse>("/watch/gameserver", "{}", HttpMethod.Get, ct);
}

        public Task Reserve(int seconds, CancellationToken ct = default)
        {            
            var json = JsonSerializer.Serialize(new ReserveBody(seconds));
            return SendRequestAsync<NullResponse>("/reserve", json, ct);
        }

        public Task Label(string key, string value, CancellationToken ct = default)
        {            
            var json = JsonSerializer.Serialize(new KeyValueMessage(key, value));
            return SendRequestAsync<NullResponse>("/metadata/label", json, HttpMethod.Put, ct);
        }

        public Task Annotation(string key, string value, CancellationToken ct = default)
        {            
            var json = JsonSerializer.Serialize(new KeyValueMessage(key, value));
            return SendRequestAsync<NullResponse>("/metadata/annotation", json, HttpMethod.Put, ct);
        }

        public async Task HealthCheckAsync(CancellationToken ct = default)
        {
            while (HealthEnabled)
            {
                if (ct.IsCancellationRequested) throw new OperationCanceledException();
                await Health(ct).ConfigureAwait(false);
                await Task.Delay(_settings.HealthInterval, ct);
            }
        }

        private Task<TResponse> SendRequestAsync<TResponse>(string api, string json, CancellationToken ct) where TResponse : class 
            => SendRequestAsync<TResponse>(api, json, HttpMethod.Post, ct);
        private async Task<TResponse> SendRequestAsync<TResponse>(string api, string json, HttpMethod method, CancellationToken ct) where TResponse : class
        {
            TResponse response = null;
            if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

            var httpClient = _httpClientFactory.CreateClient(_settings.HttpClientName);
            httpClient.BaseAddress = SideCarAddress;
            var requestMessage = new HttpRequestMessage(method, api);
            if (_settings.CacheRequest)
            {
                if (jsonCache.Value.TryGetValue(json, out var cachedContent))
                {
                    requestMessage.Content = cachedContent;
                }
                else
                {
                    var stringContent = new StringContent(json, encoding, "application/json");
                    stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    jsonCache.Value.TryAdd(json, stringContent);
                    requestMessage.Content = stringContent;
                }
            }
            else
            {
                var stringContent = new StringContent(json, encoding, "application/json");
                stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                requestMessage.Content = stringContent;
            }
            var res = await httpClient.SendAsync(requestMessage, ct).ConfigureAwait(false);
            
            // result
            var content = await res.Content.ReadAsByteArrayAsync();
            if (content != null)
            {
                response = JsonSerializer.Deserialize<TResponse>(content);
            }
            return response;
        }

        public class ReserveBody
        {
            public int Seconds { get; set; }
            public ReserveBody(int seconds) => Seconds = seconds;
        }

        public class KeyValueMessage
        {
            public string Key;
            public string Value;
            public KeyValueMessage(string key, string value) => (Key, Value) = (key, value);
        }
    }
}
