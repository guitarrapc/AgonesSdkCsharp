namespace AgonesSdkCsharp;

public class MockAgonesSdk : IAgonesSdk
{
    public bool IsRunningOnKubernetes => true;
    public bool HealthEnabled { get; set; } = true;
    public AgonesSdkOptions Options { get; } = new AgonesSdkOptions();

    private readonly GameServerResponse mockResponse;

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
            Ports = [
                new PortInfo
                {
                    Name = "http",
                    Port = 8080,
                }
            ],
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
            Annotations = [
                new Annotation
                {
                    Key = "key",
                    Value = "value",
                },
            ],
            Labels = [
                new Label
                {
                    Key = "key",
                    Value = "value",
                },
            ],
        };
        var response = new GameServerResponse()
        {
            ObjectMeta = mockResponseObjectMeta,
            Status = mockResponseStatus,
        };
        return response;
    }

    public async Task Ready(CancellationToken ct = default) { }

    public async Task Allocate(CancellationToken ct = default) { }

    public async Task Shutdown(CancellationToken ct = default) { }

    public async Task Reserve(int seconds, CancellationToken ct = default) { }

    public async Task Health(CancellationToken ct = default) { }

    public async Task SetLabel(string key, string value, CancellationToken ct = default) { }

    public async Task SetAnnotation(string key, string value, CancellationToken ct = default) { }

    public async Task<GameServerResponse> GameServer(CancellationToken ct = default) => mockResponse;

    public void WatchGameServer(Action<GameServerResponse> onResponse, CancellationToken ct = default) { }
}
