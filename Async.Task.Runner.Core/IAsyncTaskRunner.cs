namespace Async.Task.Runner.Core
{
    public interface IAsyncTaskRunner<T>
    {
        Task<Guid> StartTaskAsync(Func<Task<T>> taskFunc);
        Task<T> GetTaskResultByTaskIdAsync(Guid taskId);
    }
}
