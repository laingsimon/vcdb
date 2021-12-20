using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    public class ProcessHelper : IProcessHelper
    {
        private readonly Dictionary<string, string> commandPathCache = new Dictionary<string, string>();

        public async Task<string> GetFullPathToCommand(string command, CancellationToken cancellationToken)
        {
            if (!commandPathCache.ContainsKey(command))
            {
                commandPathCache.Add(command, await GetFullPath(command, cancellationToken));
            }

            return commandPathCache[command];
        }

        private static async Task<string> GetFullPath(string command, CancellationToken cancellationToken)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Environment.GetEnvironmentVariable("comspec") ?? throw new InvalidOperationException("Unable to retrieve comspec environment variable"),
                    Arguments = $"/c \"where {command}\"",
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
                throw new InvalidOperationException($"Unable to locate command `{command}`: {errorMessages}");
            }

            try
            {
                string path = null;
                string line;
                while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        path = line;
                        continue;
                    }

                    if (line.EndsWith(".exe") && !path.EndsWith(".exe"))
                    {
                        // prefer .exe paths over paths without an extension
                        path = line;
                    }
                }

                return path;
            }
            finally
            {
                await process.StandardOutput.ReadToEndAsync();    
            }
        }
    }
}