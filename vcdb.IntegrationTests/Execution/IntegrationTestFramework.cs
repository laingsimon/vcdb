using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
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
        private readonly IIntegrationTestExecutionContext executionContext;
        private readonly IDocker docker;
        private readonly TaskGate taskGate;
        private readonly ScenarioFilter scenarioFilter;
        private readonly IServiceProvider serviceProvider;

        public IntegrationTestFramework(
            IntegrationTestOptions options,
            IServiceProvider serviceProvider,
            ISql sql,
            IIntegrationTestExecutionContext executionContext,
            IDocker docker,
            TaskGate taskGate,
            ScenarioFilter scenarioFilter)
        {
            this.options = options;
            this.serviceProvider = serviceProvider;
            this.sql = sql;
            this.executionContext = executionContext;
            this.docker = docker;
            this.taskGate = taskGate;
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

            if (!await docker.IsContainerRunning())
            {
                await docker.StartDockerCompose();
            }

            await sql.WaitForReady(attempts: 10);

            var scenarios = scenariosDirectory
                .EnumerateDirectories()
                .Where(d => d.Name == options.ScenarioName || (options.ScenarioName == null && scenarioFilter.IsValidScenario(d)))
                .ToArray();

            options.StandardOutput.WriteLine($"Executing {scenarios.Length} scenario/s...");

            var tasks = scenarios.Select(scenarioDirectory => ExecuteScenario(new Scenario(scenarioDirectory), connectionString)).ToArray();
            await Task.WhenAll(tasks);

            executionContext.Finished();
            return scenarios.Length;
        }

        private async Task ExecuteScenario(Scenario scenario, string connectionString)
        {
            using (taskGate.StartTask())
            using (var scope = serviceProvider.CreateScope())
            {
                var scenarioScope = scope.ServiceProvider.GetRequiredService<ScenarioScope>();
                scenarioScope.Scenario = scenario;

                var scenarioExecutor = scope.ServiceProvider.GetRequiredService<ScenarioExecutor>();

                var logMessage = $" - {scenario.Name}...";

                options.StandardOutput.WriteLine(logMessage);

                try
                {
                    await scenarioExecutor.Execute(scenario, connectionString);
                }
                catch (AssertionException)
                {
                    throw;
                }
                catch (Exception exc)
                {
                    executionContext.ScenarioComplete(scenario, IntegrationTestStatus.Exception, new[] { exc.Message });
                }
            }
        }
    }
}
