using Microsoft.Extensions.DependencyInjection;
using vcdb.SchemaBuilding;
using vcdb.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerInstaller : IServicesInstaller
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ITableRepository, SqlServerTableRepository>();
            services.AddSingleton<IColumnsRepository, SqlServerColumnsRepository>();
            services.AddSingleton<IIndexesRepository, SqlServerIndexesRepository>();
            services.AddSingleton<IDatabaseScriptBuilder, SqlServerDatabaseScriptBuilder>();
            services.AddSingleton<ITableScriptBuilder, SqlServerTableScriptBuilder>();
            services.AddSingleton<ISqlObjectNameHelper, SqlObjectNameHelper>();
        }
    }
}
