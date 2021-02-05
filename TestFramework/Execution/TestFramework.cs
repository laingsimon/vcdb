using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestFramework.Database;
using TestFramework.Input;
using TestFramework.Output;

namespace TestFramework.Execution
{
    internal class TestFramework : ITestFramework
    {
        private readonly Options options;
        private readonly ILogger logger;
        private readonly ISql sql;
        private readonly ExecutionContext executionContext;
        private readonly IDocker docker;
        private readonly ITaskGate taskGate;
        private readonly IServiceProvider serviceProvider;

        public TestFramework(
            Options options,
            ILogger logger,
            IServiceProvider serviceProvider,
            ISql sql,
            ExecutionContext executionContext,
            IDocker docker,
            ITaskGate taskGate)
        {
            this.options = options;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.sql = sql;
            this.executionContext = executionContext;
            this.docker = docker;
            this.taskGate = taskGate;
        }

        public async Task Execute()
        {
            if (!docker.IsInstalled())
            {
                logger.LogError("Docker is not installed");
                return;
            }

            if (!await docker.IsDockerHostRunning())
            {
                await docker.StartDockerHost();
            }

            var scenariosDirectory = string.IsNullOrEmpty(options.ScenariosPath)
                ? new DirectoryInfo(Directory.GetCurrentDirectory())
                : new DirectoryInfo(options.ScenariosPath);

            if (!await docker.IsContainerRunning("testframework_sqlserver_1"))
            {
                var frameworkDirectory = Path.GetFullPath(Path.Combine(scenariosDirectory.FullName, "..\\TestFramework"));
                await docker.StartDockerCompose(frameworkDirectory);
            }

            await sql.WaitForReady(attempts: 10);

            if (!scenariosDirectory.Exists)
            {
                logger.LogError($"Scenarios directory not found: {scenariosDirectory.FullName}");
                return;
            }

            var scenarios = scenariosDirectory
                .EnumerateDirectories()
                .Where(DirectoryNotExcluded)
                .Where(DirectoryIncluded)
                .ToArray();

            logger.LogInformation($"Executing {scenarios.Length} scenario/s...");

            var tasks = scenarios.Select(scenarioDirectory => ExecuteScenario(scenarioDirectory)).ToArray();
            await Task.WhenAll(tasks);

            executionContext.Finished();
        }

        private async Task ExecuteScenario(DirectoryInfo scenarioDirectory)
        {
            using (taskGate.StartTask())
            using (var scope = serviceProvider.CreateScope())
            {
                var scenarioDirectoryFactory = scope.ServiceProvider.GetRequiredService<ScenarioDirectoryFactory>();
                scenarioDirectoryFactory.ScenarioDirectory = scenarioDirectory;

                var scenarioExecutor = scope.ServiceProvider.GetRequiredService<IScenarioExecutor>();

                var logMessage = $" - {scenarioDirectory.Name}...";
                OutputDetail messageDetail;

                using (logger.GetWriteLock())
                {
                    messageDetail = logger.LogLine(logMessage);
                }
                var executionResult = (ExecutionResultStatus)0;

                try
                {
                    executionResult = await scenarioExecutor.Execute(scenarioDirectory);
                }
                catch (Exception exc)
                {
                    executionContext.ScenarioComplete(scenarioDirectory, ExecutionResultStatus.Exception, new[] { exc.Message });
                }
                finally
                {
                    if (!Console.IsOutputRedirected && !options.Porcelain)
                    {
                        using (logger.GetWriteLock())
                        using (new ResetCursorPosition(messageDetail?.EndingConsoleLeft - 3, messageDetail?.EndingConsoleTop - 1))
                        using (new ResetConsoleColor(foreground: GetConsoleColor(executionResult)))
                        {
                            Console.Write($" {executionResult.ToString().ToLower()}");
                        }
                    }
                }
            }
        }

        private ConsoleColor GetConsoleColor(ExecutionResultStatus resultStatus)
        {
            switch (resultStatus)
            {
                case ExecutionResultStatus.Pass:
                    return ConsoleColor.DarkGreen;
                case ExecutionResultStatus.Timeout:
                    return ConsoleColor.Yellow;
                default:
                    return ConsoleColor.Red;
            }
        }

        private bool DirectoryNotExcluded(DirectoryInfo scenarioDirectory)
        {
            return string.IsNullOrEmpty(options.ExcludeScenarioFilter)
                || !Regex.IsMatch(scenarioDirectory.Name, options.ExcludeScenarioFilter);
        }

        private bool DirectoryIncluded(DirectoryInfo scenarioDirectory)
        {
            return string.IsNullOrEmpty(options.IncludeScenarioFilter)
                || Regex.IsMatch(scenarioDirectory.Name, options.IncludeScenarioFilter);
        }
    }
}
