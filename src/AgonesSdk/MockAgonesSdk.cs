using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgonesSdk
{
    public class MockAgonesSdk : IAgonesSdk
    {
        public bool HealthEnabled { get; set; } = true;
        public AgonesSdkOptions Options => new AgonesSdkOptions();

        public Task Allocate(CancellationToken ct = default)
        {
            return Task.FromResult(true);
        }

        public Task<GameServerResponse> GameServer(CancellationToken ct = default)
        {
            return Task.FromResult<GameServerResponse>(null);
        }
        public Task<GameServerResponse> Watch(CancellationToken ct = default)
        {
            // todo: stream どうするの?
            return Task.FromResult<GameServerResponse>(null);
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
