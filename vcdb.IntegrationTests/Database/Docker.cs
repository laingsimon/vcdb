using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;

namespace vcdb.IntegrationTests.Database
{
    internal class Docker : IDocker
    {
        public const string DockerComposeFolderName = "vcdb.IntegrationTests";

        private static readonly string DockerDesktopPath = EnvironmentVariable.Get<string>("DockerDesktopPath") ??  "C:\\Program Files\\Docker\\Docker\\Docker Desktop.exe";
        private readonly IntegrationTestOptions options;

        public Docker(IntegrationTestOptions options)
        {
            this.options = options;
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
                Debug.WriteLine(process.StandardError.ReadToEnd());
                return false;
            }

            Debug.WriteLine(process.StandardOutput.ReadToEnd());
            return true;
        }

        public async Task<bool> IsContainerRunning(ProductName productName)
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
                Console.WriteLine(process.StandardError.ReadToEnd());
                return false;
            }

            var stdOut = process.StandardOutput.ReadToEnd();
            var containerName = GetContainerName(productName);
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

            Console.WriteLine("Starting docker desktop...");
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

        public async Task<bool> StartDockerCompose(ProductName productName, CancellationToken cancellationToken = default)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = "/c \"docker-compose up\"",
                    WorkingDirectory = GetDockerComposeDirectoryPath(),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            Console.WriteLine($"Starting docker-compose in {process.StartInfo.WorkingDirectory}");

            var errorData = new StringBuilder();
            process.ErrorDataReceived += (sender, args) =>
            {
                Console.Error.WriteLine(args.Data);
            };

            var dockerContainerStarted = new ManualResetEvent(false);
            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                    return;

                var lines = args.Data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (Regex.IsMatch(line, @"^Attaching to") && Regex.IsMatch(line, GetContainerName(productName)))
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

        private string GetDockerComposeDirectoryPath()
        {
            var scenariosDirectory = string.IsNullOrEmpty(options.ScenariosPath)
                ? new DirectoryInfo(Directory.GetCurrentDirectory())
                : new DirectoryInfo(options.ScenariosPath);

            return Path.Combine(scenariosDirectory.FullName, @$"..\{DockerComposeFolderName}");
        }

        private string GetContainerName(ProductName productName, string suffix = "_1")
        {
            var dockerComposeDirectoryName = Path.GetFileName(GetDockerComposeDirectoryPath());

            var normalisedDirectoryName = dockerComposeDirectoryName.Replace(".", "").ToLower();
            var normalisedProductName = productName.ToString().ToLower();

            return $"{normalisedDirectoryName}_{normalisedProductName}{suffix}";
        }
    }
}
