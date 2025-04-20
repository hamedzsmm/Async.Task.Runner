using Microsoft.Extensions.DependencyInjection;

namespace Async.Task.Runner.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(typeof(IAsyncTaskRunner<>), typeof(InMemoryAsyncTaskRunner<>));
            return serviceCollection;
        }
    }
}
