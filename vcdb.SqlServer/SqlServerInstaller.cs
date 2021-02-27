using Microsoft.Extensions.DependencyInjection;
using vcdb.CommandLine;
using vcdb.DependencyInjection;
using vcdb.SqlServer.SchemaBuilding;
using vcdb.SqlServer.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerInstaller : IServicesInstaller
    {
        public void RegisterServices(IServiceCollection services, DatabaseVersion databaseVersion)
        {
            services.InNamespace<SqlServerDatabaseDetailsProvider>().AddAsSingleton();
            services.InNamespace<SqlServerDatabaseRepository>().AddAsSingleton();
            services.InNamespace<SqlServerDatabaseScriptBuilder>().AddAsSingleton();

            ObjectName.Converter = new ObjectNameConverter(
                delimiter: ".",
                startName: "[",
                endName: "]");
        }
    }
}
