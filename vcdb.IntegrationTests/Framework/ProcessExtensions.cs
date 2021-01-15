using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Framework
{
    internal static class ProcessExtensions
    {
        public static async Task<bool> WaitForExitAsync(this Process process, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var timeoutTime = timeout == null
                    ? default(DateTime?)
                    : DateTime.UtcNow.Add(timeout.Value);

                var checkFrequency = TimeSpan.FromSeconds(0.25);
                while (!cancellationToken.IsCancellationRequested)
                {
                    var exited = process.WaitForExit(checkFrequency);
                    if (exited)
                        return true;

                    if (timeoutTime.HasValue && DateTime.UtcNow > timeoutTime)
                        return exited;
                }

                throw new TaskCanceledException();
            }, cancellationToken);
        }

        public static bool WaitForExit(this Process process, TimeSpan timeout)
        {
            return process.WaitForExit((int)timeout.TotalMilliseconds);
        }
    }
}
