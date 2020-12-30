using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests
{
    public class ProcessExecutor
    {
        public async Task<ProcessExecutionResult> ExecuteProcess(string commandLineArguments)
        {
            var workingDirectory = Path.GetFullPath("..\\..\\..\\..\\TestScenarios");
            if (!Directory.Exists(workingDirectory))
                throw new DirectoryNotFoundException($"WorkingDirectory could not be found: {workingDirectory}");

            var executable = Path.GetFullPath(Path.Combine(workingDirectory, "..\\TestFramework\\bin\\Debug\\netcoreapp3.1\\TestFramework.dll"));
            if (!File.Exists(executable))
                throw new FileNotFoundException($"Executable could not be found: {executable}", executable);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = $"/c \"dotnet \"{executable}\" {commandLineArguments} \"",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            var result = new ProcessExecutionResult();
            process.OutputDataReceived += (sender, args) =>
            {
                result.StdOut.Add(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                result.StdErr.Add(args.Data);
            };

            if (!process.Start())
                throw new InvalidOperationException($"Could not start process: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            result.ExitCode = process.ExitCode;
            return result;
        }
    }
}
