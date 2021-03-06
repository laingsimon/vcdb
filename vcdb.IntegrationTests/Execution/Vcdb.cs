﻿using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.IntegrationTests.Content;
using vcdb.IntegrationTests.Output;
using vcdb.Output;
using vcdb.Scripting.Database;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.IntegrationTests.Execution
{
    internal class Vcdb
    {
        private readonly IJson json;
        private readonly IDatabaseProduct databaseProduct;
        private readonly IntegrationTestOptions options;

        public Vcdb(IJson json, IDatabaseProduct databaseProduct, IntegrationTestOptions options)
        {
            this.json = json;
            this.databaseProduct = databaseProduct;
            this.options = options;
        }

        public async Task<VcdbExecutionResult> Execute(ScenarioSettings settings, Scenario scenario, string connectionString)
        {
            var standardOutput = new StringWriter();
            var errorOutput = new StringWriter();

            var result = new VcdbExecutionResult
            {
                ExitCode = 0
            };

            var options = GetOptions(scenario, settings, connectionString);

            if (!string.IsNullOrEmpty(options.InputFile))
            {
                ReformatInputFile(options.InputFile);
            }

            await Program.ExecuteAsync(
                options,
                new IntegrationTestOutputFactory(new UndisposableTextWriter(standardOutput)),
                code => result.ExitCode = code,
                new UndisposableTextWriter(errorOutput),
                new UndisposableTextWriter(standardOutput),
                true,
                services => ModifyServices(services, scenario));

            result.Output = standardOutput.GetStringBuilder().ToString();
            result.ErrorOutput = errorOutput.GetStringBuilder().ToString();
            result.CommandLine = $"dotnet vcdb.dll {BuildCommandLine(options)}";

            return result;
        }

        private void ReformatInputFile(string inputFile)
        {
            var jsonContent = json.ReadJsonFromFile<object>(inputFile);
            json.WriteJsonContent(jsonContent, inputFile, Formatting.Indented);
        }

        private void ModifyServices(IServiceCollection services, Scenario scenario)
        {
            services.AddSingleton(scenario);
            ReplaceSingleton<IDatabaseComparer, EmittingDatabaseComparer>(services);
            services.AddSingleton<DatabaseComparer>();

            ReplaceSingleton<IScriptExecutionPlanManager, InterceptingScriptExecutionPlanManager>(services);
            services.AddSingleton<ScriptExecutionPlanManager>();

            services.AddSingleton(databaseProduct);
        }

        private static IEnumerable<string> GetCommandLineArguments(Scenario scenario, string connectionString)
        {
            yield return "--connectionString";
            yield return connectionString;
            yield return "--database";
            yield return scenario.DatabaseName;
            if (scenario.DatabaseVersion != null)
            {
                yield return "--type";
                yield return scenario.DatabaseVersion.ToString();
            }
        }

        private Options GetOptions(Scenario scenario, ScenarioSettings settings, string connectionString)
        {
            var commandLine = GetCommandLineArguments(scenario, connectionString)
                .Concat(StringExtensions.SplitCommandLine(settings.CommandLine));
            if (settings.Mode != null)
            {
                commandLine = commandLine.Concat(new[] {
                    "--mode",
                    settings.Mode.ToString()
                }).ToArray();
            }

            var options = new Options
            {
                WorkingDirectory = this.options.FileGroup.DirectoryPath.FullName,
                AssemblySearchPaths = new[] { Path.GetFullPath(Path.Combine(typeof(Vcdb).Assembly.Location, "..", "..", "..", "..", "..", "vcdb", "bin", BuildConfiguration.Current, "netcoreapp3.1")) },
                InputFile = settings.Mode == ExecutionMode.Deploy
                    ? GetInputFileName(scenario)
                    : null
            };

            var result = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
            }).ParseArguments(() => options, commandLine);

            if (result.Tag == ParserResultType.Parsed)
                return options;

            throw new InvalidOperationException("Unable to parse commandline");
        }

        private string GetInputFileName(Scenario scenario)
        {
            return scenario.FindFile($"Input.json");
        }

        private static IServiceCollection ReplaceSingleton<TInterface, TInstance>(IServiceCollection services)
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(TInterface), typeof(TInstance), ServiceLifetime.Singleton);
            return services.Replace(serviceDescriptor);
        }

        private string BuildCommandLine(CommandLine.Options options)
        {
            return Parser.Default.FormatCommandLine(options);
        }

        private class IntegrationTestOutputFactory : IOutputFactory
        {
            private readonly TextWriter standardOut;

            public IntegrationTestOutputFactory(TextWriter standardOut)
            {
                this.standardOut = standardOut;
            }

            public TextWriter GetActualConsoleOutput()
            {
                return standardOut;
            }
        }
    }
}
