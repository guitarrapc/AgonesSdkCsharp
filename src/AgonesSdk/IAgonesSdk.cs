using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgonesSdk
{
    // should implement: ready,allocate,setlabel,setannotation,gameserver,health,shutdown,watch
    // https://agones.dev/site/docs/guides/client-sdks/#rest-api-implementation
    // spec: https://github.com/googleforgames/agones/blob/release-1.0.0/sdk.swagger.json
    // ref: sdk server https://github.com/googleforgames/agones/blob/deab3ce0e521a98231a0ca00834276431980e7e1/pkg/sdk/sdk.pb.go#L546
    public interface IAgonesSdk
    {
        bool HealthEnabled { get; set; }
        AgonesSdkOptions Options { get; }

        /// <summary>
        /// Call when the GameServer is ready
        /// </summary>
        /// <remarks>/Ready</remarks>
        /// <returns></returns>
        Task Ready(CancellationToken ct);
        /// <summary>
        /// Call to self Allocation the GameServer (POST)
        /// </summary>
        /// <remarks>/Allocate</remarks>
        /// <returns></returns>
        Task Allocate(CancellationToken ct);
        /// <summary>
        /// Call when the GameServer is shutting down
        /// </summary>
        /// <remarks>/Shutdown</remarks>
        /// <returns></returns>
        Task Shutdown(CancellationToken ct);
        /// <summary>
        /// Send a Empty every d Duration to declare that this GameSever is healthy
        /// </summary>
        /// <remarks>/Health (stream)</remarks>
        /// <returns></returns>
        Task Health(CancellationToken ct);
        /// <summary>
        /// Retrieve the current GameServer data
        /// </summary>
        /// <remarks>/GetGameServer</remarks>
        /// <returns></returns>
        Task<GameServerResponse> GameServer(CancellationToken ct);
        /// <summary>
        /// Send GameServer details whenever the GameServer is updated
        /// </summary>
        /// <remarks>/WatchGameServer (stream)</remarks>
        /// <returns></returns>
        Task<GameServerResponse> Watch(CancellationToken ct);
        /// <summary>
        /// Apply a Label to the backing GameServer metadata
        /// </summary>
        /// <remarks>/SetLabel</remarks>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task Label(string key, string value, CancellationToken ct);
        /// <summary>
        /// Apply a Annotation to the backing GameServer metadata
        /// </summary>
        /// <remarks>/SetAnnotation</remarks>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task Annotation(string key, string value, CancellationToken ct);
        /// <summary>
        /// Marks the GameServer as the Reserved state for Duration
        /// </summary>
        /// <remarks>/Reserve</remarks>
        /// <returns></returns>
        Task Reserve(int seconds, CancellationToken ct);
    }
}
