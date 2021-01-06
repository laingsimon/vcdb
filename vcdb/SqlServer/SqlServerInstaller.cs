using Microsoft.Extensions.DependencyInjection;
using vcdb.SchemaBuilding;
using vcdb.Scripting;
using vcdb.SqlServer.SchemaBuilding;
using vcdb.SqlServer.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerInstaller : IServicesInstaller
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IDescriptionRepository, SqlServerDescriptionRepository>();
            services.AddSingleton<IDatabaseRepository, SqlServerDatabaseRepository>();
            services.AddSingleton<ITableRepository, SqlServerTableRepository>();
            services.AddSingleton<IColumnsRepository, SqlServerColumnsRepository>();
            services.AddSingleton<IIndexesRepository, SqlServerIndexesRepository>();
            services.AddSingleton<ISchemaRepository, SqlServerSchemaRepository>();
            services.AddSingleton<IDatabaseScriptBuilder, DatabaseScriptBuilder>();
            services.AddSingleton<ITableScriptBuilder, SqlServerTableScriptBuilder>();
            services.AddSingleton<ISqlObjectNameHelper, SqlObjectNameHelper>();
            services.AddSingleton<ISchemaScriptBuilder, SqlServerSchemaScriptBuilder>();
        }
    }
}
