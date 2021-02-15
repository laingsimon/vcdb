﻿using JsonEqualityComparer;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Comparison;
using vcdb.IntegrationTests.Content;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests.Execution
{
    internal class ScenarioExecutor
    {
        private readonly ISql sql;
        private readonly IJson json;
        private readonly IntegrationTestExecutionContext executionContext;
        private readonly IJsonEqualityComparer jsonEqualityComparer;
        private readonly IntegrationTestOptions options;
        private readonly IScriptDiffer differ;
        private readonly Vcdb vcdbProcess;
        private readonly IDifferenceFilter differenceFilter;
        private readonly IDatabaseProduct databaseProduct;

        public ScenarioExecutor(
            ISql sql,
            IJson json,
            IntegrationTestExecutionContext executionContext,
            IJsonEqualityComparer jsonEqualityComparer,
            IntegrationTestOptions options,
            IScriptDiffer differ,
            Vcdb vcdbProcess,
            IDifferenceFilter differenceFilter,
            IDatabaseProduct databaseProduct)
        {
            this.sql = sql;
            this.json = json;
            this.executionContext = executionContext;
            this.jsonEqualityComparer = jsonEqualityComparer;
            this.options = options;
            this.differ = differ;
            this.vcdbProcess = vcdbProcess;
            this.differenceFilter = differenceFilter;
            this.databaseProduct = databaseProduct;
        }

        public async Task<IntegrationTestStatus> Execute(DirectoryInfo scenario, string connectionString)
        {
            try
            {
                await InitialiseDatabase(scenario);
            }
            catch (Exception exc)
            {
                return executionContext.ScenarioComplete(scenario, IntegrationTestStatus.InitialiseDatabaseError, new[] { $"Unable to initialise the database: {exc.Message}" });
            }
            var settings = ReadScenarioSettings(scenario) ?? ScenarioSettings.Default;

            var result = await vcdbProcess.Execute(settings, scenario, connectionString);
            if (settings.ExpectedExitCode.HasValue && result.ExitCode != settings.ExpectedExitCode.Value)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, IntegrationTestStatus.UnexpectedExitCode, new[] { $"Expected process to exit with code {settings.ExpectedExitCode}, but it exited with {result.ExitCode}", result.ErrorOutput });
            }
            else if (result.Timeout)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, IntegrationTestStatus.Timeout, new[] { $"vcdb process did not exit within the given timeout", result.ErrorOutput });
            }
            else if (result.ExitCode != 0)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, IntegrationTestStatus.UnexpectedExitCode, new[] { $"vcdb process exited with non-success exit code: {result.ExitCode}", result.ErrorOutput });
            }

            if (settings.Mode == null || settings.Mode.Equals("Read", StringComparison.OrdinalIgnoreCase))
            {
                return await CompareJsonResult(settings, result, scenario);
            }
            else
            {
                var executionResult = await CompareSqlScriptResult(result, scenario);
                var actualOutputFilePath = Path.Combine(scenario.FullName, $"ActualOutput.{databaseProduct.Name}.sql");

                if (executionResult == IntegrationTestStatus.Pass)
                {
                    var scriptExecutionResult = await TestSqlScriptResult(settings, result, scenario);
                    if (scriptExecutionResult == IntegrationTestStatus.Pass)
                    {
                        File.Delete(actualOutputFilePath);
                        try
                        {
                            await DropDatabase(scenario);
                        }
                        catch (Exception exc)
                        {
                            Console.Error.WriteLine($"Unable to drop database for {scenario.Name}\r\n{exc}");
                        }

                        return IntegrationTestStatus.Pass;
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

        private void PrintReproductionStatement(DirectoryInfo scenario, VcdbExecutionResult result)
        {
            Console.WriteLine($"Execute vcdb with the following commandline to debug this scenario:\r\n{scenario.FullName}\r\n$ {result.CommandLine}");
        }

        private async Task<IntegrationTestStatus> TestSqlScriptResult(ScenarioSettings settings, VcdbExecutionResult result, DirectoryInfo scenario)
        {
            try
            {
                Debug.WriteLine("Testing created sql script...");
                await sql.ExecuteBatchedSql(new StringReader(result.Output), scenario.Name);

                return executionContext.ScenarioComplete(scenario, IntegrationTestStatus.Pass, new string[0] { });
            }
            catch (SqlException exc)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, IntegrationTestStatus.InvalidSql, new[] { exc.Message });
            }
            catch (Exception exc)
            {
                PrintReproductionStatement(scenario, result);
                return executionContext.ScenarioComplete(scenario, IntegrationTestStatus.Exception, new[] { exc.ToString() });
            }
        }

        private async Task<IntegrationTestStatus> CompareSqlScriptResult(VcdbExecutionResult result, DirectoryInfo scenario)
        {
            using (var expectedReader = new StreamReader(Path.Combine(scenario.FullName, $"ExpectedOutput.{databaseProduct.Name}.sql")))
            {
                var differences = differ.CompareScripts(await expectedReader.ReadToEndAsync(), result.Output);
                var filteredDifferences = differenceFilter.FilterDifferences(differences).ToArray();
                var executionResult = filteredDifferences.Any()
                    ? IntegrationTestStatus.Different
                    : IntegrationTestStatus.Pass;

                if (executionResult != IntegrationTestStatus.Pass)
                {
                    executionContext.ScenarioComplete(
                        scenario,
                        executionResult,
                        filteredDifferences.SelectMany(difference => difference.GetLineDifferences(databaseProduct)));
                }

                return executionResult;
            }
        }

        private Task<IntegrationTestStatus> CompareJsonResult(ScenarioSettings settings, VcdbExecutionResult result, DirectoryInfo scenario)
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
                        ? IntegrationTestStatus.Different
                        : IntegrationTestStatus.Pass,
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
                    ? IntegrationTestStatus.Different
                    : IntegrationTestStatus.Pass;
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
            await Retry(async () => await sql.ExecuteBatchedSql(new StringReader(databaseProduct.InitialiseDatabase(scenario.Name))));

            var databaseInitialisationFile = scenario.GetFiles($"Database.{databaseProduct.Name}.sql").SingleOrDefault();
            if (databaseInitialisationFile != null)
            {
                await sql.ExecuteBatchedSql(databaseInitialisationFile.OpenText(), scenario.Name);
            }
            else
            {
                Console.WriteLine($"{scenario.Name}: Database.{databaseProduct.Name}.sql was not found in the database directory, the database will be empty when the scenario executes");
            }
        }

        private async Task DropDatabase(DirectoryInfo scenario)
        {
            if (options.KeepDatabases)
                return;

            await Retry(async () => await databaseProduct.DropDatabase(scenario.Name, sql));
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
                    Debug.WriteLine(exc.Message);

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