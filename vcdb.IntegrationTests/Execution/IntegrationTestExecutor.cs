using DiffPlex.DiffBuilder;
using JsonEqualityComparer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Comparison;
using vcdb.IntegrationTests.Content;
using vcdb.IntegrationTests.Database;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestExecutor
    {
        public async Task<IntegrationTestResult> ExecuteScenarios(IntegrationTestOptions options)
        {
            var result = new IntegrationTestResult();
            var errorOutput = new ListWrappingWriter(result.StdErr);

            try
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton(options);
                ConfigureServices(serviceCollection, result, options, errorOutput);

                using (var serviceProvider = serviceCollection.BuildServiceProvider())
                {
                    var executor = serviceProvider.GetRequiredService<IntegrationTestFramework>();

                    try
                    {
                        var scenarios = await executor.Execute(options.ConnectionString);

                        var context = serviceProvider.GetRequiredService<IntegrationTestExecutionContext>();

                        result.ExitCode = context.Fail; //report the number of failures in the exit code
                    }
                    catch (Exception exc)
                    {
                        result.ExitCode = -1;
                        errorOutput.WriteLine(exc.ToString());
                    }
                }
            }
            catch (Exception exc)
            {
                result.ExitCode = -2;
                errorOutput.WriteLine(exc.ToString());
            }

            return result;
        }

        private static void ConfigureServices(ServiceCollection services, IntegrationTestResult result, IntegrationTestOptions options, TextWriter errorWriter)
        {
            var outputWriter = new ListWrappingWriter(result.StdOut);

            services.AddSingleton(options.ProductName);
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
            services.ReplaceSingleton<ILogger, IntegrationTestLogger>(new IntegrationTestLogger(outputWriter, errorWriter, options.MinLogLevel));

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
