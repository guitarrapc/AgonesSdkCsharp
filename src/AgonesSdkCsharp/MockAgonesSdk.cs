using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgonesSdkCsharp
{
    public class MockAgonesSdk : IAgonesSdk
    {
        public bool IsRunningOnKubernetes => true;
        public bool HealthEnabled { get; set; } = true;
        public AgonesSdkOptions Options { get; } = new AgonesSdkOptions();

        private readonly GameServerResponse mockResponse;

        public MockAgonesSdk() { }
        public MockAgonesSdk(AgonesSdkOptions options)
        {
            Options = options;
            this.mockResponse = CreateMockResponse();
        }
        public MockAgonesSdk(AgonesSdkOptions options, GameServerResponse mockResponse)
        {
            Options = options;
            this.mockResponse = mockResponse;
        }

        public static GameServerResponse CreateMockResponse()
        {
            var mockResponseStatus = new Status
            {
                Address = "127.0.0.1",
                Ports = new[] {
                        new PortInfo
                        {
                            Name = "http",
                            Port = 8080,
                        }
},
                State = "Ready",
            };
            var mockResponseObjectMeta = new ObjectMeta
            {
                Name = "mock",
                Namespace = "default",
                Generation = "gen1",
                ResourceVersion = "v1",
                Uid = "0",
                CreationTimestamp = new DateTime(2020, 1, 1, 0, 0, 0).ToString("yyyyMMdd_HHMMss"),
                Annotations = new[]
                    {
                        new Annotation
                        {
                            Key = "key",
                            Value = "value",
                        },
                    },
                Labels = new[]
                    {
                        new Label
                        {
                            Key = "key",
                            Value = "value",
                        },
                    },
            };
            var response = new GameServerResponse()
            {
                ObjectMeta = mockResponseObjectMeta,
                Status = mockResponseStatus,
            };
            return response;
        }

        public Task Allocate(CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }

        public Task<GameServerResponse> GameServer(CancellationToken ct = default)
        {
            return Task.FromResult<GameServerResponse>(mockResponse);
        }
        public Task<GameServerResponse> Watch(CancellationToken ct = default)
        {
            return Task.FromResult<GameServerResponse>(mockResponse);
        }

        public Task Health(CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }

        public Task Ready(CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }

        public Task Reserve(int seconds, CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }

        public Task Annotation(string key, string value, CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }

        public Task Label(string key, string value, CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }

        public Task Shutdown(CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }
    }
}
