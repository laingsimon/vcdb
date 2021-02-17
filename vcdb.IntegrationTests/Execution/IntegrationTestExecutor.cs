using DiffPlex.DiffBuilder;
using JsonEqualityComparer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Comparison;
using vcdb.IntegrationTests.Content;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestExecutor
    {
        private readonly bool reformatJson;

        public IntegrationTestExecutor(bool reformatJson)
        {
            this.reformatJson = reformatJson;
        }

        public async Task ExecuteScenario(IntegrationTestOptions options)
        {
            var allOutput = new StringWriter();
            var executionContext = new SingleIntegrationTestExecutionContext(allOutput);
            options.StandardOutput = allOutput;
            options.ErrorOutput = allOutput;

            await Execute(options, executionContext);
        }

        public async Task<MultipleIntegrationTestExecutionContext> ExecuteScenarios(IntegrationTestOptions options)
        {
            var executionContext = new MultipleIntegrationTestExecutionContext(options.StandardOutput, options.ErrorOutput);
            await Execute(options, executionContext);
            return executionContext;
        }

        private async Task Execute(IntegrationTestOptions options, IIntegrationTestExecutionContext executionContext)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(options);
            ConfigureServices(serviceCollection, options, executionContext);

            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var executor = serviceProvider.GetRequiredService<IntegrationTestFramework>();

                var scenarios = await executor.Execute(options.ConnectionString);
            }
        }

        private void ConfigureServices(ServiceCollection services, IntegrationTestOptions options, IIntegrationTestExecutionContext executionContext)
        {
            services.AddSingleton(options.DatabaseProduct);
            services.AddSingleton<IntegrationTestFramework>();
            services.AddSingleton<ISql, Sql>();
            services.AddSingleton(executionContext);
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

            services.AddSingleton(new JsonSerializer
            {
                Converters =
                {
                    new StringEnumConverter()
                }
            });

            services.AddScoped<ScenarioExecutor>();
            services.AddScoped<Vcdb>();
            if (reformatJson)
            {
                services.AddScoped<Json>();
                services.AddScoped<IJson, ReformattingJson>();
            }
            else
            {
                services.AddScoped<IJson, Json>();
            }
            services.AddScoped<ScenarioFactory>();
            services.AddScoped(factory => factory.GetRequiredService<ScenarioFactory>().Scenario);
        }
    }
}
