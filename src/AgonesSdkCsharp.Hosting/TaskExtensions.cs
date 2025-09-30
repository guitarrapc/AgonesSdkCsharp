using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AgonesSdkCsharp.Hosting;

internal static class TaskExtensions
{
    public static void FireAndForget(this Task task, ILogger logger = null)
    {
        task.ContinueWith(x =>
        {
            logger?.LogError("TaskUnhandled", x.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
