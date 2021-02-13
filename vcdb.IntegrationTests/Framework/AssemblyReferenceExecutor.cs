using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using TestFramework.Comparison;
using TestFramework.Execution;
using TestFramework.Input;
using TestFramework.Output;

namespace vcdb.IntegrationTests.Framework
{
    internal class AssemblyReferenceExecutor : IExecutor
    {
        public async Task<ExecutorResult> ExecuteProcess(string scenarioName = null)
        {
            var options = new Options
            {
                ConnectionString = TestScenarios.ConnectionString,
                IncludeScenarioFilter = string.IsNullOrEmpty(scenarioName)
                    ? null
                    : $"^{scenarioName}$",
                MaxConcurrency = 10,
                Porcelain = true,
                ScenariosPath = Path.GetFullPath("..\\..\\..\\..\\TestScenarios"),
                MinLogLevel = LogLevel.Information,
                UseLocalDatabase = TestScenarios.UseLocalDatabase
            };
            var result = new ExecutorResult();

            await TestFramework.Program.ExecuteWithOptions(
                options,
                services => ModifyServices(services, result, options.MinLogLevel),
                new ListWrappingWriter(result.StdErr),
                code => result.ExitCode = code);

            return result;
        }

        private static void ModifyServices(ServiceCollection services, ExecutorResult result, LogLevel minLogLevel)
        {
            var outputWriter = new ListWrappingWriter(result.StdOut);
            var errorWriter = new ListWrappingWriter(result.StdErr);

            services.ReplaceSingleton<ILogger, IntegrationTestLogger>(new IntegrationTestLogger(outputWriter, errorWriter, minLogLevel));
            services.ReplaceSingleton<IVcdbProcess, VcdbIntegrationTestProcess>();
            services.ReplaceSingleton<IScriptDiffer, HeaderCommentIgnoringScriptDiffer>();
            services.AddSingleton<ScriptDiffer>();
        }
    }
}
