using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestFramework
    {
        private readonly IntegrationTestOptions options;
        private readonly ISql sql;
        private readonly IntegrationTestExecutionContext executionContext;
        private readonly IDocker docker;
        private readonly TaskGate taskGate;
        private readonly ProductName productName;
        private readonly ScenarioFilter scenarioFilter;
        private readonly IServiceProvider serviceProvider;

        public IntegrationTestFramework(
            IntegrationTestOptions options,
            IServiceProvider serviceProvider,
            ISql sql,
            IntegrationTestExecutionContext executionContext,
            IDocker docker,
            TaskGate taskGate,
            ProductName productName,
            ScenarioFilter scenarioFilter)
        {
            this.options = options;
            this.serviceProvider = serviceProvider;
            this.sql = sql;
            this.executionContext = executionContext;
            this.docker = docker;
            this.taskGate = taskGate;
            this.productName = productName;
            this.scenarioFilter = scenarioFilter;
        }

        public async Task<int> Execute(string connectionString)
        {
            if (!docker.IsInstalled())
            {
                throw new InvalidOperationException("Docker is not installed");
            }

            if (!await docker.IsDockerHostRunning())
            {
                await docker.StartDockerHost();
            }

            var scenariosDirectory = string.IsNullOrEmpty(options.ScenariosPath)
                ? new DirectoryInfo(Directory.GetCurrentDirectory())
                : new DirectoryInfo(options.ScenariosPath);

            if (!scenariosDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Scenarios directory not found: {scenariosDirectory.FullName}");
            }

            if (!await docker.IsContainerRunning(productName))
            {
                await docker.StartDockerCompose(productName);
            }

            await sql.WaitForReady(attempts: 10);

            var scenarios = scenariosDirectory
                .EnumerateDirectories()
                .Where(d => d.Name == options.ScenarioName || (options.ScenarioName == null && scenarioFilter.IsValidScenario(d)))
                .ToArray();

            Console.WriteLine($"Executing {scenarios.Length} scenario/s...");

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

                Console.WriteLine(logMessage);
                var executionResult = (IntegrationTestStatus)0;

                try
                {
                    executionResult = await scenarioExecutor.Execute(scenarioDirectory, connectionString);
                }
                catch (Exception exc)
                {
                    executionContext.ScenarioComplete(scenarioDirectory, IntegrationTestStatus.Exception, new[] { exc.Message });
                }
            }
        }
    }
}
