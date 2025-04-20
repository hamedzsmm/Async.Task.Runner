using Async.Task.Runner.Core.TestApp.Services.GeoService.Models;

namespace Async.Task.Runner.Core.TestApp.Services.GeoService
{
    public class GeoService : IGeoService
    {
        public async Task<LocationInfo> GetLocationAsync(double latitude, double longitude)
        {
            //simulate long running task
            await System.Threading.Tasks.Task.Delay(1500);
            return new LocationInfo { CountryId = 100, CityId = 101, VillageId = 1011 };
        }
    }
}
