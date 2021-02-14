using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestFramework
    {
        private readonly IntegrationTestOptions options;
        private readonly ILogger logger;
        private readonly ISql sql;
        private readonly IntegrationTestExecutionContext executionContext;
        private readonly IDocker docker;
        private readonly TaskGate taskGate;
        private readonly ProductName productName;
        private readonly IServiceProvider serviceProvider;

        public IntegrationTestFramework(
            IntegrationTestOptions options,
            ILogger logger,
            IServiceProvider serviceProvider,
            ISql sql,
            IntegrationTestExecutionContext executionContext,
            IDocker docker,
            TaskGate taskGate,
            ProductName productName)
        {
            this.options = options;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.sql = sql;
            this.executionContext = executionContext;
            this.docker = docker;
            this.taskGate = taskGate;
            this.productName = productName;
        }

        public async Task<int> Execute(string connectionString)
        {
            if (!docker.IsInstalled())
            {
                logger.LogError("Docker is not installed");
                return -1;
            }

            if (!await docker.IsDockerHostRunning())
            {
                await docker.StartDockerHost();
            }

            var scenariosDirectory = string.IsNullOrEmpty(options.ScenariosPath)
                ? new DirectoryInfo(Directory.GetCurrentDirectory())
                : new DirectoryInfo(options.ScenariosPath);

            if (!await docker.IsContainerRunning($"testframework_{productName.ToString().ToLower()}_1"))
            {
                var frameworkDirectory = Path.GetFullPath(Path.Combine(scenariosDirectory.FullName, "..\\TestFramework"));
                await docker.StartDockerCompose(frameworkDirectory, productName);
            }

            await sql.WaitForReady(attempts: 10);

            if (!scenariosDirectory.Exists)
            {
                logger.LogError($"Scenarios directory not found: {scenariosDirectory.FullName}");
                return -2;
            }

            var scenarios = scenariosDirectory
                .EnumerateDirectories()
                .Where(DirectoryNotExcluded)
                .Where(DirectoryIncluded)
                .Where(directory => directory.GetFiles($"ExpectedOutput.{productName}.sql").Any() || directory.GetFiles($"ExpectedOutput.json").Any())
                .ToArray();

            logger.LogInformation($"Executing {scenarios.Length} scenario/s...");

            var tasks = scenarios.Select(scenarioDirectory => ExecuteScenario(scenarioDirectory, connectionString)).ToArray();
            await Task.WhenAll(tasks);

            executionContext.Finished();
            return scenarios.Length;
        }

        private async Task ExecuteScenario(DirectoryInfo scenarioDirectory, string connectionString)
        {
            using (taskGate.StartTask())
            using (var scope = serviceProvider.CreateScope())
            {
                var scenarioDirectoryFactory = scope.ServiceProvider.GetRequiredService<ScenarioDirectoryFactory>();
                scenarioDirectoryFactory.ScenarioDirectory = scenarioDirectory;

                var scenarioExecutor = scope.ServiceProvider.GetRequiredService<ScenarioExecutor>();

                var logMessage = $" - {scenarioDirectory.Name}...";

                logger.LogLine(logMessage);
                var executionResult = (ExecutionResultStatus)0;

                try
                {
                    executionResult = await scenarioExecutor.Execute(scenarioDirectory, connectionString);
                }
                catch (Exception exc)
                {
                    executionContext.ScenarioComplete(scenarioDirectory, ExecutionResultStatus.Exception, new[] { exc.Message });
                }
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
