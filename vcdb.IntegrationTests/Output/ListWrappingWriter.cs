using System.Collections.Generic;
using System.IO;
using System.Text;

namespace vcdb.IntegrationTests.Output
{
    internal class ListWrappingWriter : TextWriter
    {
        private readonly List<string> output;

        public ListWrappingWriter(List<string> output)
        {
            this.output = output;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string line)
        {
            output.Add(line);
        }
    }
}
