using JsonEqualityComparer;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestFramework
{
    internal class ScenarioExecutor : IScenarioExecutor
    {
        private readonly ILogger<ScenarioExecutor> log;
        private readonly ISql sql;
        private readonly IJson json;
        private readonly ExecutionContext executionContext;
        private readonly IJsonEqualityComparer jsonEqualityComparer;
        private readonly Options options;

        public ScenarioExecutor(
            ILogger<ScenarioExecutor> log, 
            ISql sql, 
            IJson json, 
            ExecutionContext executionContext,
            IJsonEqualityComparer jsonEqualityComparer,
            Options options)
        {
            this.log = log;
            this.sql = sql;
            this.json = json;
            this.executionContext = executionContext;
            this.jsonEqualityComparer = jsonEqualityComparer;
            this.options = options;
        }

        public async Task Execute(DirectoryInfo scenario)
        {
            log.LogInformation($"Executing scenario: {scenario.Name}");
            await InitialiseDatabase(scenario);
            var settings = ReadScenarioSettings(scenario) ?? ScenarioSettings.Default;

            var result = await ExecuteCommandLine(settings, scenario);

            if (settings.ExpectedExitCode.HasValue && result.ExitCode != settings.ExpectedExitCode.Value)
            {
                throw new InvalidOperationException($"Expected process to exit with code {settings.ExpectedExitCode}, but it exited with {result.ExitCode}");
            }

            if (string.IsNullOrEmpty(result.Output))
            {
                throw new InvalidOperationException("Process exited with null or empty content");
            }

            if (settings.Mode == null || settings.Mode == vcdb.ExecutionMode.Construct)
            {
                await CompareJsonResult(settings, result, scenario);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private async Task CompareJsonResult(ScenarioSettings settings, ExecutionResult result, DirectoryInfo scenario)
        {
            var actual = json.ReadJsonContent(result.Output);
            var expected = json.ReadJsonFromFile("ExpectedOutput.json");
            var context = new ComparisonContext
            {
                DefaultComparisonOptions = settings.JsonComparison ?? new ComparisonOptions { PropertyNameComparer = StringComparison.OrdinalIgnoreCase }
            };

            jsonEqualityComparer.Compare(actual, expected, context);

            executionContext.ScenarioComplete(
                scenario, 
                !context.Differences.Any(),
                context.Differences.Select(difference => $"- Found a difference: {difference}"));
        }

        private async Task<ExecutionResult> ExecuteCommandLine(ScenarioSettings settings, DirectoryInfo scenario)
        {
            var vcdbBuildConfiguration = settings.VcDbBuildConfiguraton ?? "Debug";
            var fileName = settings.VcDbPath ?? $@"../../vcdb/bin/{vcdbBuildConfiguration}/netcoreapp3.1/vcdb.dll";
            var commandLine = $"dotnet \"{fileName}\" --mode {settings.Mode} {settings.CommandLine.Replace("{ConnectionString}", options.ConnectionString)}";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = $"/c \"{commandLine}\"",
                    WorkingDirectory = scenario.FullName,
                    RedirectStandardOutput = true
                }
            };

            try
            {
                if (!process.Start())
                {
                    throw new InvalidOperationException($"Unable to start process `{process.StartInfo.FileName} {process.StartInfo.Arguments}`");
                }

                process.WaitForExit();
                var output = await process.StandardOutput.ReadToEndAsync();

                return new ExecutionResult
                {
                    Output = output,
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception exc)
            {
                throw new InvalidOperationException($"Unable to execute process (`{process.StartInfo.FileName} {process.StartInfo.Arguments}`): {exc.Message}", exc);
            }
        }

        private ScenarioSettings ReadScenarioSettings(DirectoryInfo scenario)
        {
            var scenarioSettingsFile = scenario.GetFiles("Scenario.json").SingleOrDefault();
            if (scenarioSettingsFile == null)
                return null;

            return json.ReadJsonFromFile<ScenarioSettings>(scenarioSettingsFile.Name);
        }

        private async Task InitialiseDatabase(DirectoryInfo scenario)
        {
            var databaseInitialisationFile = scenario.GetFiles("Database.sql").SingleOrDefault();
            if (databaseInitialisationFile != null)
            {
                await sql.ExecuteBatchedSql(databaseInitialisationFile.OpenText());
            }
        }
    }
}
