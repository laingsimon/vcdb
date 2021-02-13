using CommandLine;
using DiffPlex.DiffBuilder;
using JsonEqualityComparer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TestFramework.Comparison;
using TestFramework.Content;
using TestFramework.Database;
using TestFramework.Execution;
using TestFramework.Input;
using TestFramework.Output;

[assembly: InternalsVisibleTo("vcdb.IntegrationTests")]

namespace TestFramework
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
            }).ParseArguments<Options>(args)
              .WithParsedAsync(ExecuteWithOptions).Wait();
        }

        internal static async Task ExecuteWithOptions(Options o)
        {
            await ExecuteWithOptions(o, null, Console.Error, code => Environment.ExitCode = code);
        }

        internal static async Task ExecuteWithOptions(Options o, Action<ServiceCollection> modifyServices, TextWriter errorOutput, Action<int> setExitCode)
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton(o);
                ConfigureServices(serviceCollection, o.UseLocalDatabase);
                modifyServices?.Invoke(serviceCollection);

                using (var serviceProvider = serviceCollection.BuildServiceProvider())
                {
                    var executor = serviceProvider.GetRequiredService<ITestFramework>();

                    try
                    {
                        await executor.Execute();

                        var context = serviceProvider.GetRequiredService<ExecutionContext>();
                        setExitCode(context.Fail); //report the number of failures in the exit code
                    }
                    catch (Exception exc)
                    {
                        setExitCode(-1);
                        errorOutput.WriteLine(exc.ToString());
                    }
                }
            }
            catch (Exception exc)
            {
                setExitCode(-2);
                errorOutput.WriteLine(exc.ToString());
            }
        }

        private static void ConfigureServices(ServiceCollection services, bool useLocalDatabase)
        {
            services.AddSingleton<ITestFramework, Execution.TestFramework>();
            services.AddSingleton<ISql, Sql>();
            services.AddScoped<IJson, Json>();
            services.AddScoped<ScenarioDirectoryFactory>();
            services.AddScoped(factory => factory.GetRequiredService<ScenarioDirectoryFactory>().ScenarioDirectory);
            services.AddSingleton<ExecutionContext>();
            services.AddSingleton<IJsonEqualityComparer, Comparer>();
            services.AddSingleton<IInlineDiffBuilder>(InlineDiffBuilder.Instance);
            services.AddSingleton(
                typeof(IDocker),
                useLocalDatabase
                    ? typeof(NullDocker)
                    : typeof(Docker));
            services.AddSingleton<ILogger, ConsoleLogger>();
            services.AddSingleton<ITaskGate, TaskGate>();
            services.AddSingleton<IVcdbProcess, VcdbProcess>();
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

            services.AddScoped<IScenarioExecutor, ScenarioExecutor>();
        }
    }
}
