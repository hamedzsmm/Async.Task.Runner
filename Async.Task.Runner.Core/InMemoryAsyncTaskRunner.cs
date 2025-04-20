namespace Async.Task.Runner.Core
{
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    public class InMemoryAsyncTaskRunner<T>(IMemoryCache memoryCache) : IAsyncTaskRunner<T>
    {
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<T>> _taskCompletionSources = new();

        public Task<Guid> StartTaskAsync(Func<Task<T>> taskFunc)
        {
            var taskId = Guid.NewGuid();
            var tcs = new TaskCompletionSource<T>();
            _taskCompletionSources[taskId] = tcs;

            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await taskFunc();
                    var options = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                    };
                    memoryCache.Set(taskId.ToString(), result, options);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    _taskCompletionSources.TryRemove(taskId, out _);
                }
            });

            return Task.FromResult(taskId);
        }


        public async Task<T> GetTaskResultByTaskIdAsync(Guid taskId)
        {
            var cacheKey = taskId.ToString();
            var cached = memoryCache.Get(cacheKey);

            if (cached is T result)
            {
                return result;
            }

            if (_taskCompletionSources.TryGetValue(taskId, out var tcs))
            {
                return await tcs.Task;
            }

            throw new KeyNotFoundException("Task not found or expired.");
        }
    }

}
