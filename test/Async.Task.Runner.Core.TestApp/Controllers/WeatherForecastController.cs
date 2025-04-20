using Async.Task.Runner.Core.TestApp.Services.GeoService;
using Async.Task.Runner.Core.TestApp.Services.GeoService.Models;
using Microsoft.AspNetCore.Mvc;

namespace Async.Task.Runner.Core.TestApp.Controllers
{
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

            return result;
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

            return result;
        }
    }
}
