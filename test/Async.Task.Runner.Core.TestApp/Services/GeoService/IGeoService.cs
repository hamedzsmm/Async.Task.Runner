using Async.Task.Runner.Core.TestApp.Services.GeoService.Models;

namespace Async.Task.Runner.Core.TestApp.Services.GeoService
{
    public interface IGeoService
    {
        Task<LocationInfo> GetLocationAsync(double latitude, double longitude);
    }
}
