using JsonEqualityComparer;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestFramework.Comparison;
using TestFramework.Content;
using TestFramework.Database;
using TestFramework.Input;
using TestFramework.Output;

namespace TestFramework.Execution
{
    internal class ScenarioExecutor : IScenarioExecutor
    {
        private readonly ILogger log;
        private readonly ISql sql;
        private readonly IJson json;
        private readonly ExecutionContext executionContext;
        private readonly IJsonEqualityComparer jsonEqualityComparer;
        private readonly Options options;
        private readonly IScriptDiffer differ;
        private readonly IVcdbProcess vcdbProcess;

        public ScenarioExecutor(
            ILogger log,
            ISql sql,
            IJson json,
            ExecutionContext executionContext,
            IJsonEqualityComparer jsonEqualityComparer,
            Options options,
            IScriptDiffer differ,
            IVcdbProcess vcdbProcess)
        {
            this.log = log;
            this.sql = sql;
            this.json = json;
            this.executionContext = executionContext;
            this.jsonEqualityComparer = jsonEqualityComparer;
            this.options = options;
            this.differ = differ;
            this.vcdbProcess = vcdbProcess;
        }

        public async Task<bool> Execute(DirectoryInfo scenario)
        {
            await InitialiseDatabase(scenario);
            var settings = ReadScenarioSettings(scenario) ?? ScenarioSettings.Default;

            var result = await vcdbProcess.Execute(settings, scenario);
            if (settings.ExpectedExitCode.HasValue && result.ExitCode != settings.ExpectedExitCode.Value)
            {
                PrintReproductionStatement(scenario, result);
                executionContext.ScenarioComplete(scenario, false, new[] { $"Expected process to exit with code {settings.ExpectedExitCode}, but it exited with {result.ExitCode}", result.ErrorOutput });
                return false;
            }
            else if (result.ExitCode != 0)
            {
                PrintReproductionStatement(scenario, result);
                executionContext.ScenarioComplete(scenario, false, new[] { $"vcdb process exited with non-success exit code: {result.ExitCode}", result.ErrorOutput });
                return false;
            }

            if (settings.Mode == null || settings.Mode.Equals("Read", StringComparison.OrdinalIgnoreCase))
            {
                return await CompareJsonResult(settings, result, scenario);
            }
            else
            {
                var pass = await CompareSqlScriptResult(result, scenario);
                var actualOutputFilePath = Path.Combine(scenario.FullName, "ActualOutput.sql");

                if (pass)
                {
                    if (await TestSqlScriptResult(settings, result, scenario))
                    {
                        File.Delete(actualOutputFilePath);
                        await DropDatabase(scenario);
                        return true;
                    }

                    File.WriteAllText(actualOutputFilePath, result.Output);
                    return false;
                }
                else
                {
                    File.WriteAllText(actualOutputFilePath, result.Output);
                    PrintReproductionStatement(scenario, result);
                    return false;
                }
            }
        }

        private void PrintReproductionStatement(DirectoryInfo scenario, ExecutionResult result)
        {
            log.LogInformation($"Execute vcdb with the following commandline to debug this scenario:\r\n{scenario.FullName}\r\n$ {result.CommandLine}");
        }

        private async Task<bool> TestSqlScriptResult(ScenarioSettings settings, ExecutionResult result, DirectoryInfo scenario)
        {
            try
            {
                log.LogDebug("Testing created sql script...");
                await sql.ExecuteBatchedSql(new StringReader(result.Output), scenario.Name);

                executionContext.ScenarioComplete(scenario, true, new string[0] { });
                return true;
            }
            catch (SqlException exc)
            {
                PrintReproductionStatement(scenario, result);
                executionContext.ScenarioComplete(scenario, false, new[] { exc.Message });
                return false;
            }
            catch (Exception exc)
            {
                PrintReproductionStatement(scenario, result);
                executionContext.ScenarioComplete(scenario, false, new[] { exc.ToString() });
                return false;
            }
        }

        private Task<bool> CompareSqlScriptResult(ExecutionResult result, DirectoryInfo scenario)
        {
            return Task.Run(() =>
            {
                using (var expectedReader = new StreamReader(Path.Combine(scenario.FullName, "ExpectedOutput.sql")))
                {
                    var differences = differ.CompareScripts(expectedReader, result.Output);
                    var pass = !differences.Any();

                    if (!pass)
                    {
                        executionContext.ScenarioComplete(scenario, pass, differences.SelectMany(difference => difference.GetLineDifferences()));
                    }

                    return pass;
                }
            });
        }

        private Task<bool> CompareJsonResult(ScenarioSettings settings, ExecutionResult result, DirectoryInfo scenario)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(result.Output))
                {
                    throw new InvalidOperationException($"vcdb process did not yield any standard output\r\n{result.ErrorOutput}");
                }

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

                var actualOutputFileName = Path.Combine(scenario.FullName, "ActualOutput.json");
                if (context.Differences.Any())
                {
                    json.WriteJsonContent(actual, actualOutputFileName, Formatting.Indented);
                    PrintReproductionStatement(scenario, result);
                }
                else
                {
                    File.Delete(actualOutputFileName);
                }

                return !context.Differences.Any();
            });
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
            await sql.ExecuteBatchedSql(new StringReader($@"
DROP DATABASE IF EXISTS [{scenario.Name}]
GO

CREATE DATABASE [{scenario.Name}]"));

            var databaseInitialisationFile = scenario.GetFiles("Database.sql").SingleOrDefault();
            if (databaseInitialisationFile != null)
            {
                await sql.ExecuteBatchedSql(databaseInitialisationFile.OpenText(), scenario.Name);
            }
        }

        private async Task DropDatabase(DirectoryInfo scenario)
        {
            if (options.KeepDatabases)
                return;

            try
            {
                await sql.ExecuteBatchedSql(new StringReader($@"
DROP DATABASE IF EXISTS [{scenario.Name}]
GO"), "master");
            }
            catch (Exception exc)
            {
                log.LogDebug(exc.Message);
            }
        }
    }
}
