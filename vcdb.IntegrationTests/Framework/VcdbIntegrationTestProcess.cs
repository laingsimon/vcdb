﻿using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestFramework.Execution;
using TestFramework.Input;
using vcdb.Output;
using vcdb.Scripting.Database;

namespace vcdb.IntegrationTests.Framework
{
    internal class VcdbIntegrationTestProcess : IVcdbProcess
    {
        public async Task<ExecutionResult> Execute(ScenarioSettings settings, DirectoryInfo scenario)
        {
            var standardOutput = new StringWriter();
            var errorOutput = new StringWriter();

            var result = new ExecutionResult
            {
                ExitCode = 0
            };

            var options = GetOptions(scenario, settings);

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

        private void ModifyServices(IServiceCollection services, DirectoryInfo scenario)
        {
            services.AddSingleton(scenario);
            services.ReplaceSingleton<IDatabaseComparer, EmittingDatabaseComparer>();
            services.AddSingleton<DatabaseComparer>();
        }

        private static CommandLine.Options GetOptions(DirectoryInfo scenario, ScenarioSettings settings)
        {
            var commandLine = new[]
            {
                "--mode",
                settings.Mode,
                "--connectionString",
                TestScenarios.ConnectionString,
                "--database",
                scenario.Name
            }.Concat(StringExtensions.SplitCommandLine(settings.CommandLine));

            var options = new CommandLine.Options
            {
                WorkingDirectory = scenario.FullName
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