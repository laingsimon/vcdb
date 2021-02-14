using System.Diagnostics;

namespace vcdb.IntegrationTests.Comparison
{
    [DebuggerDisplay("{Content,nq}")]
    internal class Line
    {
        public Line(int? lineNumber, string content)
        {
            LineNumber = lineNumber;
            Content = content;
        }

        public int? LineNumber { get; }
        public string Content { get; }
    }
}
