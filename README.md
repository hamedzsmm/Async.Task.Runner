# ⏱️ Async Task Runner (Generic In-Memory Background Tasks)

A lightweight, generic utility for running long-running or non-blocking tasks **in-memory** with **parallel execution**, enabling you to build high-performance APIs by offloading independent tasks without waiting for their completion upfront.

---

## 🚀 Use Case

This library is designed to solve a common problem in API design:

> ❓ *"What if I have a long-running task that I don’t immediately need the result for, but I will later in the same request or another part of the workflow?"*

Instead of awaiting the result right away, you can:

1. **Kick off the task early** using `StartTaskAsync(...)` and get a unique `TaskId`.
2. Let the task run in the background, independently.
3. Later (when the result is actually needed), call `GetTaskResultByTaskIdAsync(...)` to **wait for completion if necessary** and retrieve the result.

This allows multiple operations to proceed **in parallel**, reducing overall response time and **improving throughput** for your APIs.

---

## 🧠 Key Features

- ✅ Generic task result support (`T`)
- ✅ In-memory result caching with 1-minute expiration
- ✅ Thread-safe design with `ConcurrentDictionary` and `IMemoryCache`
- ✅ Easily pluggable via Dependency Injection in ASP.NET Core
- ✅ No external dependencies – 100% .NET built-in

---

## 💡 Example

```csharp
public class MyService
{
    private readonly IAsyncTaskRunner<string> _taskRunner;

    public MyService(IAsyncTaskRunner<string> taskRunner)
    {
        _taskRunner = taskRunner;
    }

    public async Task RunAsync()
    {
        var taskId = await _taskRunner.StartTaskAsync(async () =>
        {
            await Task.Delay(5000);
            return "Heavy work completed!";
        });

        // Do some other work here...

        var result = await _taskRunner.GetTaskResultByTaskIdAsync(taskId);
        Console.WriteLine(result); // Output: Heavy work completed!
    }
}
