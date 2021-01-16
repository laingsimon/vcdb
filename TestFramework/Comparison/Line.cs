using System.Diagnostics;

namespace TestFramework.Comparison
{
    [DebuggerDisplay("{Content,nq}")]
    public class Line
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
