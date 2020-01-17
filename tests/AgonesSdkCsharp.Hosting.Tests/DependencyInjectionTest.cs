using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AgonesSdkCsharp.Hosting.Tests
{
    public class DependencyInjectionTest
    {
        [Fact]
        public async Task AssertRegisterSdkToServiceCollection()
        {
            var options = new AgonesSdkOptions();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAgonesSdk>(new MockAgonesSdk(options))
                .BuildServiceProvider();
            var mock = serviceProvider.GetRequiredService<IAgonesSdk>();

            // Property
            Assert.True(mock.HealthEnabled);
            Assert.True(mock.Options.CacheRequest);
            Assert.Equal(TimeSpan.FromSeconds(2), mock.Options.HealthInterval);
            Assert.Equal("Agones", mock.Options.HttpClientName);
            Assert.Equal("AgonesSdkCsharp", mock.Options.HttpClientUserAgent);
            Assert.Equal(TimeSpan.FromSeconds(30), mock.Options.PollyOptions.CirtcuitBreakingDuration);
            Assert.Equal(3, mock.Options.PollyOptions.FailedRetryCount);
            Assert.Equal(5, mock.Options.PollyOptions.HandledEventsAllowedBeforeCirtcuitBreaking);

            // Method
            var cts = new CancellationTokenSource();
            Assert.Null(await Record.ExceptionAsync(() => mock.Allocate(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mock.Annotation("key", "value", cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mock.GameServer(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mock.Health(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mock.Label("key", "value", cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mock.Ready(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mock.Reserve(1, cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mock.Shutdown(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mock.Watch(cts.Token)));
        }
    }
}
