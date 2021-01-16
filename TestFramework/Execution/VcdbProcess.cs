using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TestFramework.Input;

namespace TestFramework.Execution
{
    internal class VcdbProcess : IVcdbProcess
    {
        private readonly Options options;

        public VcdbProcess(Options options)
        {
            this.options = options;
        }

        public async Task<ExecutionResult> Execute(ScenarioSettings settings, DirectoryInfo scenario)
        {
            var vcdbBuildConfiguration = settings.VcDbBuildConfiguraton ?? "Debug";
            var fileName = settings.VcDbPath ?? $@"../../vcdb/bin/{vcdbBuildConfiguration}/netcoreapp3.1/vcdb.dll";
            var commandLine = $"dotnet \"{fileName}\" --mode {settings.Mode} --database \"{scenario.Name}\" --connectionString \"{options.ConnectionString}\" {settings.CommandLine}";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = $"/c \"{commandLine}\"",
                    WorkingDirectory = scenario.FullName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = !options.ShowVcdbProgress
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
                    ErrorOutput = process.StartInfo.RedirectStandardError
                        ? await process.StandardError.ReadToEndAsync()
                        : null,
                    ExitCode = process.ExitCode,
                    CommandLine = commandLine
                };
            }
            catch (Exception exc)
            {
                throw new InvalidOperationException($"Unable to execute process (`{process.StartInfo.FileName} {process.StartInfo.Arguments}`): {exc.Message}", exc);
            }
        }
    }
}
