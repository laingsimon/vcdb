using Microsoft.Extensions.DependencyInjection;
using vcdb.DependencyInjection;
using vcdb.SqlServer.SchemaBuilding;
using vcdb.SqlServer.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerInstaller : IServicesInstaller
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.InNamespace<SqlObjectNameHelper>().AddAsSingleton();
            services.InNamespace<SqlServerCheckConstraintsRepository>().AddAsSingleton();
            services.InNamespace<SqlServerCheckConstraintScriptBuilder>().AddAsSingleton();
        }
    }
}
