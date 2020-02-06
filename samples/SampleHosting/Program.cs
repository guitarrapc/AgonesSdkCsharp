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
            //CreateHostBuilder(args).Build().Run();
            //CreateHostBuilderAgonesSettings(args).Build().Run();
            //CreateHostBuilderHttpService(args).Build().Run();
            //CreateHostBuilderHttpServiceMock(args).Build().Run();
            //Ready().GetAwaiter().GetResult();
            //CreateHostBuilderHttpServiceCustomHandler(args).Build().Run();
            CreateHostBuilderCircuiFailure(args).Build().Run();
        }

        public static async Task Ready()
        {
            var agones = new AgonesSdk(new AgonesSdkOptions(), new DummyHttpClientFactory());
            await agones.Ready();
        }

        public class DummyHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;
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

        /// <summary>
        /// Simple
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug))
            .UseAgones<AgonesSdk>(); // HealtchCheckService Log

        /// <summary>
        /// Circuit failure test
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilderCircuiFailure(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug))
            .UseAgones<CircuitMockAgonesSdk>(configureHosting: hosting => 
            {
                // circuit for 10 seconds. requests within this duration will be fail.
                hosting.CirtcuitBreakingDuration = TimeSpan.FromSeconds(10);
                hosting.HandledEventsAllowedBeforeCirtcuitBreaking = 2;
            });

        /// <summary>
        /// Pass AgonesSdkOptions
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilderAgonesSettings(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)) // HealtchCheckService Log
                .UseAgones<AgonesSdk>(configureSdk =>
                {
                    configureSdk.HealthInterval = TimeSpan.FromSeconds(1);
                    configureSdk.HttpClientName = "myAgonesClient";
                }, configureHosting =>
                {
                    configureHosting.CirtcuitBreakingDuration = TimeSpan.FromSeconds(10);
                    configureHosting.HandledEventsAllowedBeforeCirtcuitBreaking = 2;
                });
        }

        /// <summary>
        /// Use your HttpClient for AgonesSdk
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilderHttpService(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // set HttpClientName pass with settings
                    services.AddHttpClient(AgonesSdkOptions.DefaultHttpClientName, client =>
                    {
                        // you must set at least RequesetHeader. (MUST BE application/json)
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    });
                })
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)) // HealtchCheckService Log
                .UseAgones<AgonesSdk>(configureHosting => configureHosting.UseDefaultHttpClientFactory = false);
        }

        /// <summary>
        /// Use Mock AgonesSdk
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilderHttpServiceMock(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // set HttpClientName pass with settings
                    services.AddHttpClient(AgonesSdkOptions.DefaultHttpClientName, client =>
                    {
                        // you must set at least RequesetHeader. (MUST BE application/json)
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    });
                })
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)) // HealtchCheckService Log
                .UseAgones<HogeSdk>(configureSdk => configureSdk.UseDefaultHttpClientFactory = false);
        }

        /// <summary>
        /// Use custom Polly handler
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilderHttpServiceCustomHandler(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, logging) => logging.SetMinimumLevel(LogLevel.Debug)) // HealtchCheckService Log
                .UseAgones<AgonesSdk>(configureService =>
                {
                    configureService.HandledEventsAllowedBeforeCirtcuitBreaking = 2;
                });
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
        public class CircuitMockAgonesSdk : AgonesSdk
        {
            static readonly System.Text.Encoding encoding = new System.Text.UTF8Encoding(false);
            static readonly Random random = new Random();
            public CircuitMockAgonesSdk(AgonesSdkOptions options, IHttpClientFactory httpClientFactory) : base(options, httpClientFactory)
            {
            }

            public override async Task Health(CancellationToken ct = default)
            {
                await CircuitRequest(ct); // success
                // additional failure chance to raise  Circuit OnBreak.
                if (random.Next(0, 5) == 0)
                {
                    await FailureRequest(ct); // failure
                }
            }

            private Task CircuitRequest(CancellationToken ct)
            {
                return SendRequestAsync<NullResponse>("/api/circuit", "{}", HttpMethod.Post, (content) =>
                {
                    Console.WriteLine(encoding.GetString(content));
                    return null;
                }, ct);
            }
            private Task FailureRequest(CancellationToken ct)
            {
                return SendRequestAsync<NullResponse>("/api/failure", "{}", HttpMethod.Post, ct);
            }
        }
    }
}
