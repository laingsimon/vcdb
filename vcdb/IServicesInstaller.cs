using Microsoft.Extensions.DependencyInjection;

namespace vcdb
{
    public interface IServicesInstaller
    {
        void RegisterServices(IServiceCollection services);
    }
}