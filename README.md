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


## 💡 Example Use Case: Geo Lookup in the Background

Let's say you have an external geo service (`IGeoService`) that takes a latitude and longitude and returns geographic information (like country, city, village).  
This call might take a couple of seconds, and you only need the result **at the end of your API logic**, not right away.

### 🔧 Interfaces:

```csharp
public interface IGeoService
{
    Task<LocationInfo> GetLocationAsync(double latitude, double longitude);
}

public class LocationInfo
{
    public long CountryId { get; set; }
    public long CityId { get; set; }
    public long VillageId { get; set; }
}

You can offload it like this:

```csharp
public class MyService
{
    private readonly IAsyncTaskRunner<string> _taskRunner;
	private readonly IGeoService _geoService;

    public MyService(IAsyncTaskRunner<string> taskRunner,IGeoService geoService)
    {
        _taskRunner = taskRunner;
		_geoService = geoService;
    }

    public async Task RunAsync()
    {
		double latitude = (double) 35.72828545564619;
		double longtitude = (double) 51.41550287298716;
		
        var taskId = await _taskRunner.StartTaskAsync(async () =>
        {
            return await _geoService.GetLocationAsync(latitude,longtitude);
        });

        // Do some other work here...

        var locationInfo = await _taskRunner.GetTaskResultByTaskIdAsync<LocationInfo>(taskId);
        Console.WriteLine(locationInfo.CountryId); // Output: Id of Country
    }
}