﻿using System;
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
        private readonly IDatabaseProduct databaseProduct;

        public Docker(IntegrationTestOptions options, IDatabaseProduct databaseProduct)
        {
            this.options = options;
            this.databaseProduct = databaseProduct;
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

        public async Task<bool> IsContainerRunning()
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
                options.ErrorOutput.WriteLine(process.StandardError.ReadToEnd());
                return false;
            }

            var stdOut = process.StandardOutput.ReadToEnd();
            var containerName = GetContainerName();
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

            options.StandardOutput.WriteLine("Starting docker desktop...");
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

        public async Task<bool> StartDockerCompose(CancellationToken cancellationToken = default)
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

            options.StandardOutput.WriteLine($"Starting docker-compose in {process.StartInfo.WorkingDirectory}");

            var errorData = new StringBuilder();
            process.ErrorDataReceived += (sender, args) =>
            {
                options.ErrorOutput.WriteLine(args.Data);
            };

            var dockerContainerStarted = new ManualResetEvent(false);
            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                    return;

                var lines = args.Data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (Regex.IsMatch(line, @"^Attaching to") && Regex.IsMatch(line, GetContainerName()))
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
            var scenariosDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "vcdb.IntegrationTests")));

            return Path.Combine(scenariosDirectory.FullName, @$"..\{DockerComposeFolderName}");
        }

        private string GetContainerName(string suffix = "_1")
        {
            var dockerComposeDirectoryName = Path.GetFileName(GetDockerComposeDirectoryPath());

            var normalisedDirectoryName = dockerComposeDirectoryName.Replace(".", "").ToLower();
            var normalisedProductName = databaseProduct.Name.ToLower();

            return $"{normalisedDirectoryName}_{normalisedProductName}{suffix}";
        }
    }
}
