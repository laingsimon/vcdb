using JsonEqualityComparer;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
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
        private readonly IJsonEqualityComparer jsonEqualityComparer;
        private readonly IntegrationTestOptions options;
        private readonly IScriptDiffer differ;
        private readonly Vcdb vcdbProcess;
        private readonly IDifferenceFilter differenceFilter;
        private readonly IDatabaseProduct databaseProduct;
        private readonly Scenario scenario;

        public ScenarioExecutor(
            ISql sql,
            IJson json,
            IJsonEqualityComparer jsonEqualityComparer,
            IntegrationTestOptions options,
            IScriptDiffer differ,
            Vcdb vcdbProcess,
            IDifferenceFilter differenceFilter,
            IDatabaseProduct databaseProduct,
            Scenario scenario)
        {
            this.sql = sql;
            this.json = json;
            this.jsonEqualityComparer = jsonEqualityComparer;
            this.options = options;
            this.differ = differ;
            this.vcdbProcess = vcdbProcess;
            this.differenceFilter = differenceFilter;
            this.databaseProduct = databaseProduct;
            this.scenario = scenario;
        }

        public async Task Execute(string connectionString)
        {
            try
            {
                await InitialiseDatabase(scenario);
            }
            catch (Exception exc)
            {
                Assert.Fail($"Unable to initialise the database: {exc.Message}");
            }
            var settings = ReadScenarioSettings(scenario) ?? ScenarioSettings.Default;

            var result = await vcdbProcess.Execute(settings, scenario, connectionString);
            if (settings.ExpectedExitCode.HasValue && result.ExitCode != settings.ExpectedExitCode.Value)
            {
                PrintReproductionStatement(scenario, result);
                Assert.Fail($"Expected process to exit with code {settings.ExpectedExitCode}, but it exited with {result.ExitCode}\r\n{result.ErrorOutput}");
            }
            else if (result.Timeout)
            {
                PrintReproductionStatement(scenario, result);
                Assert.Fail($"vcdb process did not exit within the given timeout\r\n{result.ErrorOutput}");
            }
            else if (result.ExitCode != 0)
            {
                PrintReproductionStatement(scenario, result);
                Assert.Fail($"vcdb process exited with non-success exit code: {result.ExitCode}\r\n{result.ErrorOutput}");
            }

            if (settings.Mode == null || settings.Mode == CommandLine.ExecutionMode.Read)
            {
                await CompareJsonResult(settings, result, scenario);
            }
            else
            {
                await CompareSqlScriptResult(result, scenario);
                var actualOutputFileName = $"ActualOutput.sql";

                try
                {
                    await TestSqlScriptResult(result, scenario);
                    try
                    {
                        scenario.Delete(actualOutputFileName);
                        try
                        {
                            await DropDatabase(scenario);
                        }
                        catch (Exception exc)
                        {
                            options.ErrorOutput.WriteLine($"Unable to drop database for {scenario.DatabaseName}\r\n{exc}");
                        }

                        return;
                    }
                    catch
                    {
                        using (var actualOutputFile = scenario.Write(actualOutputFileName))
                        {
                            actualOutputFile.WriteLine(result.Output);
                        }
                        throw;
                    }
                }
                catch
                {
                    using (var actualOutputFile = scenario.Write(actualOutputFileName))
                    {
                        actualOutputFile.WriteLine(result.Output);
                    }
                    PrintReproductionStatement(scenario, result);
                    throw;
                }
            }
        }

        private void PrintReproductionStatement(Scenario scenario, VcdbExecutionResult result)
        {
            options.StandardOutput.WriteLine($"Execute vcdb with the following commandline to debug this scenario:\r\n{scenario.Directory}\r\n$ {result.CommandLine}");
        }

        private async Task TestSqlScriptResult(VcdbExecutionResult result, Scenario scenario)
        {
            try
            {
                Debug.WriteLine("Testing created sql script...");
                await sql.ExecuteBatchedSql(new StringReader(result.Output), scenario.DatabaseName);
            }
            catch
            {
                PrintReproductionStatement(scenario, result);
                throw;
            }
        }

        private async Task CompareSqlScriptResult(VcdbExecutionResult result, Scenario scenario)
        {
            using (var expectedReader = scenario.Read($"ExpectedOutput.sql"))
            {
                var differences = differ.CompareScripts(await expectedReader.ReadToEndAsync(), result.Output);
                var filteredDifferences = differenceFilter.FilterDifferences(differences).ToArray();

                if (filteredDifferences.Any())
                {
                    Assert.Fail(string.Join("\r\n", filteredDifferences.SelectMany(difference => difference.GetLineDifferences(databaseProduct))));
                }
            }
        }

        private Task CompareJsonResult(ScenarioSettings settings, VcdbExecutionResult result, Scenario scenario)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrEmpty(result.Output))
                {
                    throw new InvalidOperationException($"vcdb process did not yield any standard output\r\n{result.ErrorOutput}");
                }

                var actual = json.ReadJsonContent(result.Output);
                var expected = json.ReadJsonFromFile($"ExpectedOutput.json");
                var context = new ComparisonContext
                {
                    DefaultComparisonOptions = settings.JsonComparison ?? new ComparisonOptions { PropertyNameComparer = StringComparison.OrdinalIgnoreCase }
                };

                jsonEqualityComparer.Compare(actual, expected, context);

                var actualOutputRelativeFileName = $"ActualOutput.json";
                if (context.Differences.Any())
                {
                    json.WriteJsonContent(actual, actualOutputRelativeFileName, Formatting.Indented);
                    PrintReproductionStatement(scenario, result);
                }
                else
                {
                    scenario.Delete(actualOutputRelativeFileName);
                }

                if (context.Differences.Any())
                {
                    Assert.Fail($"Found a difference: {string.Join("\r\n", context.Differences.Select(difference => $"- {difference}"))}");
                }
            });
        }

        private ScenarioSettings ReadScenarioSettings(Scenario scenario)
        {
            return json.ReadJsonFromFile<ScenarioSettings>($"Scenario.json");
        }

        private async Task InitialiseDatabase(Scenario scenario)
        {
            await Retry(async () => await sql.ExecuteBatchedSql(new StringReader(databaseProduct.InitialiseDatabase(scenario.DatabaseName))));
            await sql.ExecuteBatchedSql(scenario.Read($"Database.sql"), scenario.DatabaseName);
        }

        private async Task DropDatabase(Scenario scenario)
        {
            if (options.KeepDatabases)
                return;

            await Retry(async () => await databaseProduct.DropDatabase(scenario.DatabaseName, sql));
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
