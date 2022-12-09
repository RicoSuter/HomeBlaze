using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HomeBlaze.Zwave
{
    public static class Retry
    {
        public static async Task RetryAsync(Func<Task> action, ILogger logger)
        {
            await RetryAsync<object?>(async () =>
            {
                await action();
                return null;
            }, logger);
        }

        public static async Task<T> RetryAsync<T>(Func<Task<T>> action, ILogger logger)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception e)
                {
                    if (i < 2)
                    {
                        logger.LogWarning(e, "An error occurred. Retrying...");
                        await Task.Delay(5000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return default!;
        }
    }
}
