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
        private readonly IDifferenceFilter differenceFilter;
        private readonly ProductName productName;

        public ScenarioExecutor(
            ILogger log,
            ISql sql,
            IJson json,
            ExecutionContext executionContext,
            IJsonEqualityComparer jsonEqualityComparer,
            Options options,
            IScriptDiffer differ,
            IVcdbProcess vcdbProcess,
            IDifferenceFilter differenceFilter,
            ProductName productName)
        {
            this.log = log;
            this.sql = sql;
            this.json = json;
            this.executionContext = executionContext;
            this.jsonEqualityComparer = jsonEqualityComparer;
            this.options = options;
            this.differ = differ;
            this.vcdbProcess = vcdbProcess;
            this.differenceFilter = differenceFilter;
            this.productName = productName;
        }

        public async Task<ExecutionResultStatus> Execute(DirectoryInfo scenario)
        {
            try
            {
                await InitialiseDatabase(scenario);
            }
            catch (Exception exc)
            {
                return executionContext.ScenarioComplete(scenario, ExecutionResultStatus.InitialiseDatabaseError, new[] { $"Unable to initialise the database: {exc.Message}" });
            }
            var settings = ReadScenarioSettings(scenario) ?? ScenarioSettings.Default;

            var result = await vcdbProcess.Execute(settings, scenario);
            if (settings.ExpectedExitCode.HasValue && result.ExitCode != settings.ExpectedExitCode.Value)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, ExecutionResultStatus.UnexpectedExitCode, new[] { $"Expected process to exit with code {settings.ExpectedExitCode}, but it exited with {result.ExitCode}", result.ErrorOutput });
            }
            else if (result.Timeout)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, ExecutionResultStatus.Timeout, new[] { $"vcdb process did not exit within the given timeout", result.ErrorOutput });
            }
            else if (result.ExitCode != 0)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, ExecutionResultStatus.UnexpectedExitCode, new[] { $"vcdb process exited with non-success exit code: {result.ExitCode}", result.ErrorOutput });
            }

            if (settings.Mode == null || settings.Mode.Equals("Read", StringComparison.OrdinalIgnoreCase))
            {
                return await CompareJsonResult(settings, result, scenario);
            }
            else
            {
                var executionResult = await CompareSqlScriptResult(result, scenario);
                var actualOutputFilePath = Path.Combine(scenario.FullName, $"ActualOutput.{productName.Name}.sql");

                if (executionResult == ExecutionResultStatus.Pass)
                {
                    var scriptExecutionResult = await TestSqlScriptResult(settings, result, scenario);
                    if (scriptExecutionResult == ExecutionResultStatus.Pass)
                    {
                        File.Delete(actualOutputFilePath);
                        try
                        {
                            await DropDatabase(scenario);
                        }
                        catch (Exception exc)
                        {
                            log.LogError(exc, $"Unable to drop database for {scenario.Name}");
                        }

                        return ExecutionResultStatus.Pass;
                    }

                    File.WriteAllText(actualOutputFilePath, result.Output);
                    return scriptExecutionResult;
                }
                else
                {
                    File.WriteAllText(actualOutputFilePath, result.Output);
                    PrintReproductionStatement(scenario, result);
                    return executionResult;
                }
            }
        }

        private void PrintReproductionStatement(DirectoryInfo scenario, ExecutionResult result)
        {
            log.LogInformation($"Execute vcdb with the following commandline to debug this scenario:\r\n{scenario.FullName}\r\n$ {result.CommandLine}");
        }

        private async Task<ExecutionResultStatus> TestSqlScriptResult(ScenarioSettings settings, ExecutionResult result, DirectoryInfo scenario)
        {
            try
            {
                log.LogDebug("Testing created sql script...");
                await sql.ExecuteBatchedSql(new StringReader(result.Output), scenario.Name);

                return executionContext.ScenarioComplete(scenario, ExecutionResultStatus.Pass, new string[0] { });
            }
            catch (SqlException exc)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, ExecutionResultStatus.InvalidSql, new[] { exc.Message });
            }
            catch (Exception exc)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, ExecutionResultStatus.Exception, new[] { exc.ToString() });
            }
        }

        private async Task<ExecutionResultStatus> CompareSqlScriptResult(ExecutionResult result, DirectoryInfo scenario)
        {
            using (var expectedReader = new StreamReader(Path.Combine(scenario.FullName, $"ExpectedOutput.{productName.Name}.sql")))
            {
                var differences = differ.CompareScripts(await expectedReader.ReadToEndAsync(), result.Output);
                var filteredDifferences = differenceFilter.FilterDifferences(differences).ToArray();
                var executionResult = filteredDifferences.Any()
                    ? ExecutionResultStatus.Different
                    : ExecutionResultStatus.Pass;

                if (executionResult != ExecutionResultStatus.Pass)
                {
                    executionContext.ScenarioComplete(
                        scenario,
                        executionResult,
                        filteredDifferences.SelectMany(difference => difference.GetLineDifferences(productName.Name)));
                }

                return executionResult;
            }
        }

        private Task<ExecutionResultStatus> CompareJsonResult(ScenarioSettings settings, ExecutionResult result, DirectoryInfo scenario)
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
                    context.Differences.Any() 
                        ? ExecutionResultStatus.Different 
                        : ExecutionResultStatus.Pass,
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

                return context.Differences.Any()
                    ? ExecutionResultStatus.Different
                    : ExecutionResultStatus.Pass;
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
            await Retry(async () => await sql.ExecuteBatchedSql(new StringReader(productName.InitialiseDatabase(scenario))));

            var databaseInitialisationFile = scenario.GetFiles($"Database.{productName.Name}.sql").SingleOrDefault();
            if (databaseInitialisationFile != null)
            {
                await sql.ExecuteBatchedSql(databaseInitialisationFile.OpenText(), scenario.Name);
            }
        }

        private async Task DropDatabase(DirectoryInfo scenario)
        {
            if (options.KeepDatabases)
                return;

            await Retry(async () => await productName.DropDatabase(scenario, sql));
        }

        private async Task Retry(Func<Task> action, int times = 3)
        {
            Exception lastException = null;
            for (var count = 0; count < times; count++)
            {
                try
                {
                    await action();

                    return;
                }
                catch (Exception exc)
                {
                    lastException = exc;
                    log.LogDebug(exc.Message);

                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                }
            }

            if (lastException != null)
            {
                throw lastException;
            }
        }
    }
}
