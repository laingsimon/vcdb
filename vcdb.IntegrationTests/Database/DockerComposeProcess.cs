using System;
using System.Diagnostics;

namespace vcdb.IntegrationTests.Database
{
    public class DockerComposeProcess : IDisposable
    {
        private readonly Process process;

        public DockerComposeProcess(Process process)
        {
            this.process = process;
        }

        public void Dispose()
        {
            if (process == null)
            {
                return;
            }

            process.StandardInput.Close();
            
            Debug.WriteLine($"Waiting for process {process.Id} to exit (`{process.StartInfo.FileName} {process.StartInfo.Arguments}`)");
            var exited = process.WaitForExit(TimeSpan.FromMinutes(1));

            if (exited)
            {
                Debug.WriteLine($"Spawned process {process.Id} has exited");
                return;
            }
            
            Debug.WriteLine($"Spawned process {process.Id} didn't exit after the tests, it may need to be terminated manually");
        }
    }
}