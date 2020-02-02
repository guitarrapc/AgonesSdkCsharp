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
            var mockSdk = serviceProvider.GetRequiredService<IAgonesSdk>();

            // Property
            Assert.True(mockSdk.HealthEnabled);
            Assert.True(mockSdk.Options.CacheRequest);
            Assert.Equal(TimeSpan.FromSeconds(2), mockSdk.Options.HealthInterval);
            Assert.Equal("Agones", mockSdk.Options.HttpClientName);
            Assert.Equal("AgonesSdkCsharp", mockSdk.Options.HttpClientUserAgent);

            // Method
            var cts = new CancellationTokenSource();
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.Allocate(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.Annotation("key", "value", cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.GameServer(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.Health(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.Label("key", "value", cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.Ready(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.Reserve(1, cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.Shutdown(cts.Token)));
            Assert.Null(await Record.ExceptionAsync(() => mockSdk.Watch(cts.Token)));
        }
    }
}
