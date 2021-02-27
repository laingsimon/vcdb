using Microsoft.Extensions.DependencyInjection;
using vcdb.CommandLine;
using vcdb.DependencyInjection;
using vcdb.MySql.SchemaBuilding;
using vcdb.MySql.Scripting;

namespace vcdb.MySql
{
    public class MySqlInstaller : IServicesInstaller
    {
        public void RegisterServices(IServiceCollection services, DatabaseVersion databaseVersion)
        {
            services.InNamespace<ConnectionFactory>().AddAsSingleton();
            services.InNamespace<MySqlDatabaseRepository>().AddAsSingleton();
            services.InNamespace<MySqlDatabaseScriptBuilder>().AddAsSingleton();

            ObjectName.Converter = new ObjectNameConverter(
                delimiter: ".",
                startName: "`",
                endName: "`");
        }
    }
}
