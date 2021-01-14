using Microsoft.Extensions.DependencyInjection;

namespace vcdb.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static ServiceNamespaceRegistrationBuilder InNamespace<T>(this IServiceCollection services)
        {
            return new ServiceNamespaceRegistrationBuilder(typeof(T), services);
        }
    }
}
