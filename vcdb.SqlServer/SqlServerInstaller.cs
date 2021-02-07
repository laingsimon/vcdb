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
            services.InNamespace<SqlObjectNameHelper>().AddAsSingleton();
            services.InNamespace<SqlServerCheckConstraintRepository>().AddAsSingleton();
            services.InNamespace<SqlServerCheckConstraintScriptBuilder>().AddAsSingleton();
        }
    }
}
