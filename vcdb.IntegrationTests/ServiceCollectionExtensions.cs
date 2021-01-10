using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace vcdb.IntegrationTests
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection ReplaceSingleton<TInterface, TInstance>(this IServiceCollection services)
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(TInterface), typeof(TInstance), ServiceLifetime.Singleton);
            return services.Replace(serviceDescriptor);
        }

        public static IServiceCollection ReplaceSingleton<TInterface, TInstance>(this IServiceCollection services, TInstance instance)
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(TInterface), instance);
            return services.Replace(serviceDescriptor);
        }
    }
}
