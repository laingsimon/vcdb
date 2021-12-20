using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;

namespace vcdb.IntegrationTests.Database
{
    internal class Docker : IDocker
    {
        private const string DockerComposeFolderName = "vcdb.IntegrationTests";
        private static readonly TimeSpan DockerComposeTimeout = TimeSpan.FromMinutes(5);
        private static readonly string DockerDesktopPath = EnvironmentVariable.Get<string>("DockerDesktopPath") ??  Environment.GetEnvironmentVariable("ProgramW6432") + "\\Docker\\Docker\\Docker Desktop.exe";

        private readonly IntegrationTestOptions options;
        private readonly IDatabaseProduct databaseProduct;
        private readonly IProcessHelper processHelper;

        public Docker(IntegrationTestOptions options, IDatabaseProduct databaseProduct, IProcessHelper processHelper)
        {
            this.options = options;
            this.databaseProduct = databaseProduct;
            this.processHelper = processHelper;
        }

        public bool IsInstalled()
        {
            return File.Exists(options.DockerDesktopPath ?? DockerDesktopPath);
        }

        public async Task<StartResult> IsDockerHostRunning(CancellationToken cancellationToken = default)
        {
            return await IsDockerHostRunning(true, cancellationToken);
        }
        
        public Task<bool> IsContainerRunning(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(databaseProduct.CanConnect(options.ConnectionString));
        }

        public async Task<bool> StartDockerHost(CancellationToken cancellationToken = default)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = options.DockerDesktopPath ?? DockerDesktopPath,
                    Arguments = "-AutoStart", // run docker desktop without showing the window in the foreground
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                }
            };

            await options.StandardOutput.WriteLineAsync("Starting docker desktop...");
            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start docker desktop - unable to start process");
            }

            var checks = 0;
            var standardOutPrintBackOff = 5;
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await IsDockerHostRunning(checks >= standardOutPrintBackOff, cancellationToken);
                checks++;
                
                switch (result)
                {
                    case StartResult.Started:
                        return true;
                    case StartResult.Unstartable:
                        return false;
                    case StartResult.NotStarted:
                        await Task.Delay(TimeSpan.FromSeconds(0.25), cancellationToken);
                        break;
                    default:
                        throw new NotSupportedException($"Response not understood: {result}");
                }
            }

            return false;
        }

        public async Task<DockerComposeProcess> StartDockerCompose(CancellationToken cancellationToken = default)
        {
            var dockerComposeFilePath = await processHelper.GetFullPathToCommand("docker-compose", cancellationToken);
            
            var process = new Process
            {
                StartInfo =
                {
                    FileName = dockerComposeFilePath,
                    Arguments = "up",
                    WorkingDirectory = GetDockerComposeDirectoryPath(),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                }
            };

            await options.StandardOutput.WriteLineAsync($"Starting docker-compose in {process.StartInfo.WorkingDirectory}");

            var dockerContainerStarted = new ManualResetEvent(false);
            var dockerComposeError = new ManualResetEvent(false);

            process.ErrorDataReceived += (_, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                {
                    return;
                }

                options.ErrorOutput.WriteLine(args.Data);
                Debug.WriteLine(args.Data);

                var lines = args.Data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Any(IsDockerComposeError))
                {
                    dockerComposeError.Set();
                }
            };

            process.OutputDataReceived += (_, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                    return;

                if (databaseProduct.CanConnect(options.ConnectionString))
                {
                    dockerContainerStarted.Set();
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start docker-compose - unable to start process");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var dockerContainerWasStarted = await Task.Run(() =>
            {
                var cancellationWaitHandle = cancellationToken.WaitHandle;
                var handleThatWasSet = WaitHandle.WaitAny(new[] { cancellationWaitHandle, dockerContainerStarted, dockerComposeError }, DockerComposeTimeout);
                
                switch (handleThatWasSet)
                {
                    case 0: //index of <cancellationWaitHandle>
                        return false;
                    case 1: // index of <dockerContainerStarted>
                        return true;
                    case 2: // index of <dockerComposeError>
                        throw new InvalidOperationException("Error encountered when executing docker-compose:\r\n" + options.ErrorOutput);
                    default:
                        throw new InvalidOperationException("Invalid index to wait handle identified");
                }
            }, cancellationToken);

            return dockerContainerWasStarted
                ? new DockerComposeProcess(process)
                : null;
        }

        private async Task<StartResult> IsDockerHostRunning(bool printStdOut, CancellationToken cancellationToken = default)
        {
            var dockerFilePath = await processHelper.GetFullPathToCommand("docker", cancellationToken);
            
            var process = new Process
            {
                StartInfo =
                {
                    FileName = dockerFilePath,
                    Arguments = "container ls",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start process");
            }

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                var errorMessages = await process.StandardError.ReadToEndAsync();
                if (string.IsNullOrEmpty(errorMessages))
                {
                    return StartResult.NotStarted;
                }

                if (printStdOut)
                {
                    await Console.Error.WriteLineAsync(errorMessages);
                }

                return StartResult.NotStarted;
            }

            if (printStdOut)
            {
                var stdOut = await process.StandardOutput.ReadToEndAsync();
                Debug.WriteLine(stdOut);
            }

            return StartResult.Started;
        }
        
        private static bool IsDockerComposeError(string line)
        {
            return Regex.IsMatch(line, @"^(.+?)\s+Error$")
                || Regex.IsMatch(line, @"^Error\s+");
        }

        private static string GetDockerComposeDirectoryPath()
        {
            var scenariosDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "vcdb.IntegrationTests")));

            return Path.Combine(scenariosDirectory.FullName, @$"..\{DockerComposeFolderName}");
        }
    }
}
