using DiffPlex.DiffBuilder;
using JsonEqualityComparer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Comparison;
using vcdb.IntegrationTests.Content;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestExecutor
    {
        public async Task ExecuteScenario(IntegrationTestOptions options, CancellationToken cancellationToken = default)
        {
            var allOutput = new StringWriter();
            options.StandardOutput = allOutput;
            options.ErrorOutput = allOutput;

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, options);

            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var executor = serviceProvider.GetRequiredService<IntegrationTestFramework>();

                await executor.Execute(options, cancellationToken);
            }
        }

        private void ConfigureServices(ServiceCollection services, IntegrationTestOptions options)
        {
            services.AddSingleton(options);
            services.AddSingleton(new Scenario(options));
            services.AddSingleton(options.DatabaseProduct);
            services.AddSingleton<IntegrationTestFramework>();
            services.AddSingleton<ISql, Sql>();
            services.AddSingleton<IJsonEqualityComparer, Comparer>();
            services.AddSingleton<IInlineDiffBuilder>(InlineDiffBuilder.Instance);
            services.AddSingleton(
                typeof(IDocker),
                options.UseLocalDatabase
                    ? typeof(NullDocker)
                    : typeof(Docker));
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

            services.AddSingleton<ScenarioExecutor>();
            services.AddSingleton<Vcdb>();
            services.AddSingleton<IJson, ReformattingJson>();
        }
    }
}
