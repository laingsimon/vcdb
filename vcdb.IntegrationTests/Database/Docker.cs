using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests.Database
{
    internal class Docker : IDocker
    {
        private static readonly string DockerDesktopPath = EnvironmentVariable.Get<string>("DockerDesktopPath") ??  "C:\\Program Files\\Docker\\Docker\\Docker Desktop.exe";
        private readonly IntegrationTestOptions options;
        private readonly ILogger logger;

        public Docker(IntegrationTestOptions options, ILogger logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public bool IsInstalled()
        {
            return File.Exists(options.DockerDesktopPath ?? DockerDesktopPath);
        }

        public async Task<bool> IsDockerHostRunning()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = "/c \"docker container ls\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start process");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                logger.LogDebug(process.StandardError.ReadToEnd());
                return false;
            }

            logger.LogDebug(process.StandardOutput.ReadToEnd());
            return true;
        }

        public async Task<bool> IsContainerRunning(string containerName)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = "/c \"docker container ls\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start process");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)

            {
                logger.LogWarning(process.StandardError.ReadToEnd());
                return false;
            }

            var stdOut = process.StandardOutput.ReadToEnd();
            return stdOut.Contains(containerName);
        }

        public async Task<bool> StartDockerHost(CancellationToken cancellationToken = default)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = options.DockerDesktopPath ?? DockerDesktopPath,
                    UseShellExecute = true,
                    LoadUserProfile = true
                }
            };

            logger.LogInformation("Starting docker desktop...");
            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start docker desktop - unable to start process");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                if (await IsDockerHostRunning())
                {
                    return true;
                }

                await Task.Delay(TimeSpan.FromSeconds(0.25));
            }

            return false;
        }

        public async Task<bool> StartDockerCompose(string workingDirectory, ProductName productName, CancellationToken cancellationToken = default)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = "/c \"docker-compose up\"",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            logger.LogInformation($"Starting docker-compose in {workingDirectory}");

            var errorData = new StringBuilder();
            process.ErrorDataReceived += (sender, args) =>
            {
                logger.LogError(args.Data);
            };

            var dockerContainerStarted = new ManualResetEvent(false);
            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                    return;

                var lines = args.Data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (Regex.IsMatch(line, $@"^Attaching to testframework_{productName}_1$"))
                    {
                        dockerContainerStarted.Set();
                    }
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
                var handleThatWasSet = WaitHandle.WaitAny(new[] { cancellationWaitHandle, dockerContainerStarted });
                return handleThatWasSet == 1; //the second wait handle
            });

            return dockerContainerWasStarted;
        }
    }
}
