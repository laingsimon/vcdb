﻿using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using JsonEqualityComparer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly IInlineDiffBuilder differ;

        public ScenarioExecutor(
            ILogger<ScenarioExecutor> log,
            ISql sql,
            IJson json,
            ExecutionContext executionContext,
            IJsonEqualityComparer jsonEqualityComparer,
            Options options,
            IInlineDiffBuilder differ)
        {
            this.log = log;
            this.sql = sql;
            this.json = json;
            this.executionContext = executionContext;
            this.jsonEqualityComparer = jsonEqualityComparer;
            this.options = options;
            this.differ = differ;
        }

        public async Task Execute(DirectoryInfo scenario)
        {
            log.LogInformation($"Executing scenario: {scenario.Name}");
            await InitialiseDatabase(scenario);
            var settings = ReadScenarioSettings(scenario) ?? ScenarioSettings.Default;

            var result = await ExecuteCommandLine(settings, scenario);
            if (settings.ExpectedExitCode.HasValue && result.ExitCode != settings.ExpectedExitCode.Value)
            {
                executionContext.ScenarioComplete(scenario, false, new[] { $"Expected process to exit with code {settings.ExpectedExitCode}, but it exited with {result.ExitCode}" });
                return;
            } 
            else if (result.ExitCode != 0)
            {
                executionContext.ScenarioComplete(scenario, false, new[] { $"vcdb process exited with non-success exit code: {result.ExitCode}" });
                return;
            }

            if (string.IsNullOrEmpty(result.Output))
            {
                executionContext.ScenarioComplete(scenario, false, new[] { "Process exited with null or empty content" });
                return;
            }

            if (settings.Mode == null || settings.Mode == vcdb.CommandLine.ExecutionMode.Construct)
            {
                await CompareJsonResult(settings, result, scenario);
            }
            else
            {
                await CompareSqlScriptResult(settings, result, scenario);
            }
        }

        private Task CompareSqlScriptResult(ScenarioSettings settings, ExecutionResult result, DirectoryInfo scenario)
        {
            return Task.Run(() =>
            {
                var actual = result.Output?.Trim() ?? "";

                using (var expectedReader = new StreamReader(Path.Combine(scenario.FullName, "ExpectedOutput.sql")))
                {
                    var expectedOutput = expectedReader.ReadToEnd().Trim();
                    var diffs = differ.BuildDiffModel(expectedOutput, actual, true, false, new LineChunker());

                    var pass = !diffs.HasDifferences;
                    var differences = FormatDiff(diffs.Lines);

                    executionContext.ScenarioComplete(scenario, pass, differences);
                }
            });
        }

        private IEnumerable<string> FormatDiff(IEnumerable<DiffPiece> lines)
        {
            var diffBlock = new List<DiffPiece>();
            DiffPiece lastDiff = null;

            foreach (var line in lines)
            {
                var captureDiff = line.Type == ChangeType.Deleted || line.Type == ChangeType.Inserted;
                if (captureDiff && lastDiff != null)
                {
                    diffBlock.Add(lastDiff);
                    lastDiff = null;
                }

                if (captureDiff)
                {
                    diffBlock.Add(line);
                    continue;
                }
                else if (diffBlock.Any())
                {
                    //add the line as the last line of context
                    diffBlock.Add(line);
                    foreach (var diffDetail in FormatDiffBlock(diffBlock))
                        yield return diffDetail;
                    diffBlock = new List<DiffPiece>();
                }

                lastDiff = line;
            }

            if (diffBlock.Any())
            {
                foreach (var diffDetail in FormatDiffBlock(diffBlock))
                    yield return diffDetail;
            }
        }

        private IEnumerable<string> FormatDiffBlock(List<DiffPiece> diffBlock)
        {
            var startingLine = diffBlock[0].Position;
            var endingLine = diffBlock.LastOrDefault(l => l.Position != null)?.Position;

            yield return $@"Lines {startingLine}..{endingLine} (vcdb output vs ExpectedOutput.json)";

            foreach (var line in diffBlock)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        yield return $"+ {line.Text}";
                        break;
                    case ChangeType.Deleted:
                        yield return $"- {line.Text}";
                        break;
                    default:
                        yield return $"  {line.Text}";
                        break;
                }
            }
        }

        private Task CompareJsonResult(ScenarioSettings settings, ExecutionResult result, DirectoryInfo scenario)
        {
            return Task.Run(() =>
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
            });
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
