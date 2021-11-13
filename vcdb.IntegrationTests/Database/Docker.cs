using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private static readonly TimeSpan dockerComposeTimeout = TimeSpan.FromMinutes(5);
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

        public async Task<StartResult> IsDockerHostRunning()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = "/c \"docker container ls\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Verb = "runas",
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException("Could not start process");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var errorMessages = process.StandardError.ReadToEnd();
                if (string.IsNullOrEmpty(errorMessages))
                {
                    return StartResult.NotStarted;
                }

                Console.Error.WriteLine(errorMessages);
                return StartResult.Unstartable;
            }

            Debug.WriteLine(process.StandardOutput.ReadToEnd());
            return StartResult.Started;
        }

        public async Task<bool> IsContainerRunning(IntegrationTestOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec"),
                    Arguments = "/c \"docker container ls\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Verb = "runas",
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
            var containerNames = new[]
            {
                GetContainerName("_"),
                GetContainerName("-"),
            };
            
            return containerNames.Any(containerName => stdOut.Contains(containerName));
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
                var result = await IsDockerHostRunning();
                if (result == StartResult.Started)
                {
                    return true;
                }

                if (result == StartResult.Unstartable)
                {
                    return false;
                }

                await Task.Delay(TimeSpan.FromSeconds(0.25));
            }

            return false;
        }

        public async Task<bool> StartDockerCompose(IntegrationTestOptions options, CancellationToken cancellationToken = default)
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

            var dockerContainerStarted = new ManualResetEvent(false);
            var dockerComposeError = new ManualResetEvent(false);

            var errorData = new StringBuilder();
            process.ErrorDataReceived += (sender, args) =>
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

            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                    return;

                var lines = args.Data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (databaseProduct.CanConnect(options.ConnectionString))
                    {
                        dockerContainerStarted.Set();
                        break;
                    }

                    if (Debugger.IsAttached)
                    {
                        Debug.WriteLine(line);
                    }

                    if ((Regex.IsMatch(line, @"^Attaching to") || Regex.IsMatch(line, @"^Container .+ Running$")) 
                        && (Regex.IsMatch(line, GetContainerName("_")) || Regex.IsMatch(line, GetContainerName("-"))))
                    {
                        dockerContainerStarted.Set();
                        break;
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
                var handleThatWasSet = WaitHandle.WaitAny(new[] { cancellationWaitHandle, dockerContainerStarted, dockerComposeError }, dockerComposeTimeout);
                
                switch (handleThatWasSet)
                {
                    case 0: //index of <cancellationWaitHandle>
                        return false;
                    case 1: // index of <dockerContainerStarted>
                        return true;
                    case 2: // index of <dockerComposeError>
                        throw new InvalidOperationException("Error encountered when executing docker-compose:\r\n" + options.ErrorOutput.ToString());
                    default:
                        throw new InvalidOperationException("Invalid index to wait handle identified");
                }
            });

            return dockerContainerWasStarted;
        }

        private static bool IsDockerComposeError(string line)
        {
            return Regex.IsMatch(line, @"^(.+?)\s+Error$")
                || Regex.IsMatch(line, @"^Error\s+");
        }

        private string GetDockerComposeDirectoryPath()
        {
            var scenariosDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "vcdb.IntegrationTests")));

            return Path.Combine(scenariosDirectory.FullName, @$"..\{DockerComposeFolderName}");
        }

        private string GetContainerName(string separator)
        {
            var normalisedProductName = databaseProduct.Name.ToLower();

            return $"vcdb{separator}{normalisedProductName}{separator}1";
        }
    }
}
