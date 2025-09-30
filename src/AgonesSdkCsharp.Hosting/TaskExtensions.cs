using Microsoft.Extensions.Logging;

namespace AgonesSdkCsharp.Hosting;

internal static class TaskExtensions
{
    public static void FireAndForget(this Task task, ILogger logger)
    {
        task.ContinueWith(x =>
        {
            logger.LogError("TaskUnhandled", x.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
