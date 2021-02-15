using DiffPlex.DiffBuilder;
using JsonEqualityComparer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Comparison;
using vcdb.IntegrationTests.Content;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestExecutor
    {
        public async Task<IntegrationTestExecutionContext> ExecuteScenarios(IntegrationTestOptions options)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(options);
            ConfigureServices(serviceCollection, options);

            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var executor = serviceProvider.GetRequiredService<IntegrationTestFramework>();

                var scenarios = await executor.Execute(options.ConnectionString);

                var context = serviceProvider.GetRequiredService<IntegrationTestExecutionContext>();
                return context;
            }
        }

        private static void ConfigureServices(ServiceCollection services, IntegrationTestOptions options)
        {
            services.AddSingleton(options.DatabaseProduct);
            services.AddSingleton<IntegrationTestFramework>();
            services.AddSingleton<ISql, Sql>();
            services.AddScoped<IJson, Json>();
            services.AddScoped<ScenarioDirectoryFactory>();
            services.AddScoped(factory => factory.GetRequiredService<ScenarioDirectoryFactory>().ScenarioDirectory);
            services.AddSingleton<IntegrationTestExecutionContext>();
            services.AddSingleton<IJsonEqualityComparer, Comparer>();
            services.AddSingleton<IInlineDiffBuilder>(InlineDiffBuilder.Instance);
            services.AddSingleton<ScenarioFilter>();
            services.AddSingleton(
                typeof(IDocker),
                options.UseLocalDatabase
                    ? typeof(NullDocker)
                    : typeof(Docker));

            services.AddSingleton<TaskGate>();
            services.AddSingleton<IScriptDiffer, HeaderCommentIgnoringScriptDiffer>();
            services.AddSingleton<ScriptDiffer>();
            services.AddSingleton<IDifferenceFilter, RegexDifferenceFilter>();
            services.AddSingleton<Vcdb>();

            services.AddSingleton(new JsonSerializer
            {
                Converters =
                {
                    new StringEnumConverter()
                }
            });

            services.AddScoped<ScenarioExecutor>();
        }
    }
}
