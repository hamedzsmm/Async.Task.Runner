using Microsoft.Extensions.DependencyInjection;

namespace Async.Task.Runner.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureTaskRunnerServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton(typeof(IAsyncTaskRunner<>), typeof(InMemoryAsyncTaskRunner<>));
            return serviceCollection;
        }
    }
}
