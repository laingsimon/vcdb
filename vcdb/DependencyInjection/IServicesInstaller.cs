using Microsoft.Extensions.DependencyInjection;

namespace vcdb.DependencyInjection
{
    public interface IServicesInstaller
    {
        void RegisterServices(IServiceCollection services, CommandLine.DatabaseVersion databaseVersion);
    }
}