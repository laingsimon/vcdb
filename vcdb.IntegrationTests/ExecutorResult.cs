using System.Collections.Generic;
using System.IO;

namespace vcdb.IntegrationTests
{
    internal class ExecutorResult
    {
        public List<string> StdOut { get; } = new List<string>();
        public List<string> StdErr { get; } = new List<string>();
        public int ExitCode { get; set; }

        public void WriteStdOutTo(TextWriter writer)
        {
            StdOut.ForEach(line => writer.WriteLine(line));
        }

        public void WriteStdErrTo(TextWriter writer)
        {
            StdErr.ForEach(line => writer.WriteLine(line));
        }
    }
}
