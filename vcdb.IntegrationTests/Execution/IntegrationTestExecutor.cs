using DiffPlex.DiffBuilder;
using JsonEqualityComparer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Comparison;
using vcdb.IntegrationTests.Content;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestExecutor
    {
        public async Task<IntegrationTestExecutionContext> ExecuteScenario(IntegrationTestOptions options, bool throwIfTestFails = true)
        {
            var allOutput = new StringWriter();
            var result = await ExecuteScenarios(options, allOutput, allOutput);
            if (result.Fail > 0 && throwIfTestFails)
                Assert.Fail(allOutput.GetStringBuilder().ToString());

            return result;
        }

        public async Task<IntegrationTestExecutionContext> ExecuteScenarios(IntegrationTestOptions options, TextWriter output = null, TextWriter error = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(options);
            ConfigureServices(serviceCollection, options, output, error);

            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var executor = serviceProvider.GetRequiredService<IntegrationTestFramework>();

                var scenarios = await executor.Execute(options.ConnectionString);

                var context = serviceProvider.GetRequiredService<IntegrationTestExecutionContext>();
                return context;
            }
        }

        private static void ConfigureServices(ServiceCollection services, IntegrationTestOptions options, TextWriter output = null, TextWriter error = null)
        {
            services.AddSingleton(options.DatabaseProduct);
            services.AddSingleton<IntegrationTestFramework>();
            services.AddSingleton<ISql, Sql>();
            services.AddScoped<IJson, Json>();
            services.AddScoped<ScenarioDirectoryFactory>();
            services.AddScoped(factory => factory.GetRequiredService<ScenarioDirectoryFactory>().ScenarioDirectory);
            services.AddSingleton(new IntegrationTestExecutionContext(output, error));
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
