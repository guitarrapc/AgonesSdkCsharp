using AgonesSdkCsharp;
using AgonesSdkCsharp.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SampleHosting
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            //CreateHostBuilderAgonesSettings(args).Build().Run();
            //CreateHostBuilderHttpService(args).Build().Run();
            //CreateHostBuilderHttpServiceMock(args).Build().Run();

            //Ready().GetAwaiter().GetResult();
        }

        public static async Task Ready()
        {
            var agones = new AgonesSdk(new AgonesSdkOptions(), new DummyHttpClientFactory());
            await agones.Ready();
        }

        public class DummyHttpClientFactory : IHttpClientFactory
        {
            private HttpClient _client;
            public DummyHttpClientFactory()
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _client = client;
            }
            public HttpClient CreateClient(string name)
            {
                return _client;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseAgones<AgonesSdk>()
            .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)); // HealtchCheckService Log

        public static IHostBuilder CreateHostBuilderAgonesSettings(string[] args)
        {
            var settings = new AgonesSdkOptions
            {
                HealthInterval = TimeSpan.FromSeconds(1),
                HttpClientName = "myAgonesClient",
                PollyOptions = new AgonesSdkHttpPollyOptions
                {
                    FailedRetryCount = 5,
                    CirtcuitBreakingDuration = TimeSpan.FromSeconds(10),
                    HandledEventsAllowedBeforeCirtcuitBreaking = 2,
                },
            };
            return Host.CreateDefaultBuilder(args)
                .UseAgones<AgonesSdk>(settings)
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)); // HealtchCheckService Log
        }

        /// <summary>
        /// Use your HttpClient for AgonesSdk
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilderHttpService(string[] args)
        {
            var settings = new AgonesSdkOptions();
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // set HttpClientName pass with settings
                    services.AddHttpClient(settings.HttpClientName, client =>
                    {
                        // you must set at least RequesetHeader. (MUST BE application/json)
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    });
                })
                .UseAgones<AgonesSdk>(settings, useDefaultHttpClientFactory: false)
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)); // HealtchCheckService Log
        }

        /// <summary>
        /// Use Mock AgonesSdk
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilderHttpServiceMock(string[] args)
        {
            var settings = new AgonesSdkOptions();
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // set HttpClientName pass with settings
                    services.AddHttpClient(settings.HttpClientName, client =>
                    {
                        // you must set at least RequesetHeader. (MUST BE application/json)
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    });
                })
                .UseAgones<HogeSdk>(settings, useDefaultHttpClientFactory: false)
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)); // HealtchCheckService Log
        }

        public class HogeSdk : IAgonesSdk
        {
            public bool HealthEnabled { get; set; }
            public AgonesSdkOptions Options { get; }

            private readonly GameServerResponse mockResponse;

            public HogeSdk(AgonesSdkOptions options) => (Options, mockResponse) = (options, CreateMockResponse());

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
}
