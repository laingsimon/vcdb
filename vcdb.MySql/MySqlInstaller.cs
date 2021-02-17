using Microsoft.Extensions.DependencyInjection;
using vcdb.CommandLine;
using vcdb.DependencyInjection;

namespace vcdb.MySql
{
    public class MySqlInstaller : IServicesInstaller
    {
        public void RegisterServices(IServiceCollection services, DatabaseVersion databaseVersion)
        {
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            //services.InNamespace<MySqlCheckConstraintRepository>().AddAsSingleton();
            //services.InNamespace<MySqlCheckConstraintScriptBuilder>().AddAsSingleton();

            ObjectName.Converter = new ObjectNameConverter(
                delimiter: ".",
                startName: "`",
                endName: "`");
        }
    }
}
