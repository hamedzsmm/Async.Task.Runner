# Async Task Runner (Generic In-Memory Background Tasks)

A lightweight, generic utility for running long-running or non-blocking tasks **in-memory** with **parallel execution**, enabling you to build high-performance APIs by offloading independent tasks without waiting for their completion upfront.

---

## Use Case

This library is designed to solve a common problem in API design:

> *"What if I have a long-running task that I don’t immediately need the result for, but I will later in the same request or another part of the workflow?"*

Instead of awaiting the result right away, you can:

1. **Kick off the task early** using `StartTaskAsync(...)` and get a unique `TaskId`.
2. Let the task run in the background, independently.
3. Later (when the result is actually needed), call `GetTaskResultByTaskIdAsync(...)` to **wait for completion if necessary** and retrieve the result.

This allows multiple operations to proceed **in parallel**, reducing overall response time and **improving throughput** for your APIs.

---

## Key Features

- ✅ Generic task result support (`T`)
- ✅ In-memory result caching with 1-minute expiration
- ✅ Thread-safe design with `ConcurrentDictionary` and `IMemoryCache`
- ✅ Easily pluggable via Dependency Injection in ASP.NET Core
- ✅ No external dependencies – 100% .NET built-in

---

## Installation
Install the package via NuGet:

```xml
dotnet add package Async.Task.Runner.Core
```

```csharp
using Async.Task.Runner.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureTaskRunnerServices();
```


## Example Use Case: Geo Lookup in the Background

Let's say you have an external geo service (`IGeoService`) that takes a latitude and longitude and returns geographic information (like country, city, village).  
This call might take a couple of seconds, and you only need the result **at the end of your API logic**, not right away.


```csharp
public interface IGeoService
{
    Task<LocationInfo> GetLocationAsync(double latitude, double longitude);
}

public class GeoService : IGeoService
{
    public async Task<LocationInfo> GetLocationAsync(double latitude, double longitude)
    {
        //simulate long running task
        await System.Threading.Tasks.Task.Delay(1500);
        return new LocationInfo { CountryId = 100, CityId = 101, VillageId = 1011 };
    }
}

public class LocationInfo
{
    public long CountryId { get; set; }
    public long CityId { get; set; }
    public long VillageId { get; set; }
}
```

# You can offload it like this:

```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    //simulate current location
    private static readonly double Latitude = (double)35.72828545564619;
    private static readonly double Longitude = (double)51.41550287298716;


    [HttpGet]
    [Route("GetWeatherForecast/normal")]
    public async Task<IEnumerable<WeatherForecast>> GetNormal([FromServices] IGeoService geoService)
    {
        await System.Threading.Tasks.Task.Delay(1000);
        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();


        var locationInfo = await geoService.GetLocationAsync(Latitude, Longitude);
        foreach (var weatherForecast in result)
        {
            weatherForecast.CountryId = locationInfo.CountryId;
        }

        return result;  //in 2.5 seconds
    }

    [HttpGet]
    [Route("GetWeatherForecast/with-task-runner-usage")]
    public async Task<IEnumerable<WeatherForecast>> GetWithTaskRunner([FromServices] IGeoService geoService,
        [FromServices] IAsyncTaskRunner<LocationInfo> locationFinderTaskRunner)
    {
        var taskId = await locationFinderTaskRunner.StartTaskAsync(async () =>
            await geoService.GetLocationAsync(Latitude, Longitude));


        await System.Threading.Tasks.Task.Delay(1000);
        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
            .ToArray();


        var locationInfo = await locationFinderTaskRunner.GetTaskResultByTaskIdAsync(taskId);
        foreach (var weatherForecast in result)
        {
            weatherForecast.CountryId = locationInfo.CountryId;
        }

        return result; //in 1.5 seconds
    }
}
```
