using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestFramework
{
    internal class TestFramework : ITestFramework
    {
        private readonly Options options;
        private readonly ILogger<TestFramework> logger;
        private readonly ISql sql;
        private readonly ExecutionContext executionContext;
        private readonly IServiceProvider serviceProvider;

        public TestFramework(
            Options options, 
            ILogger<TestFramework> logger, 
            IServiceProvider serviceProvider, 
            ISql sql,
            ExecutionContext executionContext)
        {
            this.options = options;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.sql = sql;
            this.executionContext = executionContext;
        }

        public async Task Execute()
        {
            await sql.WaitForReady(attempts: 10);

            var scenariosDirectory = string.IsNullOrEmpty(options.ScenariosPath)
                ? new DirectoryInfo(Directory.GetCurrentDirectory())
                : new DirectoryInfo(options.ScenariosPath);

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
            var scenarioNumber = 0;

            foreach (var scenarioDirectory in scenarios)
            {
                using (var scope = serviceProvider.CreateScope())
                using (var loggerScope = logger.BeginScope(scenarioDirectory.Name))
                {
                    var scenarioDirectoryFactory = scope.ServiceProvider.GetRequiredService<ScenarioDirectoryFactory>();
                    scenarioDirectoryFactory.ScenarioDirectory = scenarioDirectory;

                    var scenarioExecutor = scope.ServiceProvider.GetRequiredService<IScenarioExecutor>();
                    var log = scope.ServiceProvider.GetRequiredService<ILogger<TestFramework>>();

                    try
                    {
                        log.LogInformation($"Executing: {scenarioDirectory.Name} [{++scenarioNumber}/{scenarios.Length}]");

                        await scenarioExecutor.Execute(scenarioDirectory);
                    }
                    catch (Exception exc)
                    {
                        executionContext.ScenarioComplete(scenarioDirectory, false, new[] { exc.Message });
                        logger.LogError(exc.Message);
                    }
                }
            }

            executionContext.Finished();
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
