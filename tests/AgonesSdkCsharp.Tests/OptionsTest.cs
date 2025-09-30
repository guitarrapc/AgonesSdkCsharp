using Xunit;

namespace AgonesSdkCsharp.Tests;
public class OptionsTest
{
    [Fact]
    public void AssertOptionDefaultValue()
    {
        var options = new AgonesSdkOptions();
        // Property
        Assert.True(options.CacheRequest);
        Assert.Equal(TimeSpan.FromSeconds(2), options.HealthInterval);
        Assert.Equal("Agones", options.HttpClientName);
        Assert.Equal("AgonesSdkCsharp", options.HttpClientUserAgent);
    }

    [Fact]
    public async Task AssertSdkComplete()
    {
        var options = new AgonesSdkOptions();
        var mock = new MockAgonesSdk(options);
        var cts = new CancellationTokenSource();

        Assert.Null(await Record.ExceptionAsync(() => mock.Allocate(cts.Token)));
        Assert.Null(await Record.ExceptionAsync(() => mock.SetAnnotation("key", "value", cts.Token)));
        {
            GameServerResponse? response = null;
            var exception = await Record.ExceptionAsync(async () => response = await mock.GameServer(cts.Token));
            Assert.Null(exception);
            Assert.NotNull(response);
            Assert.NotNull(response.ObjectMeta);
            Assert.NotNull(response.Status);
        }
        Assert.Null(await Record.ExceptionAsync(() => mock.Health(cts.Token)));
        Assert.Null(await Record.ExceptionAsync(() => mock.SetLabel("key", "value", cts.Token)));
        Assert.Null(await Record.ExceptionAsync(() => mock.Ready(cts.Token)));
        Assert.Null(await Record.ExceptionAsync(() => mock.Reserve(1, cts.Token)));
        Assert.Null(await Record.ExceptionAsync(() => mock.Shutdown(cts.Token)));
        Assert.Null(Record.Exception(() => mock.WatchGameServer(response => { }, cts.Token)));
    }
}
