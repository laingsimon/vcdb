using System.Collections.Generic;
using System.Linq;

namespace vcdb.IntegrationTests.Comparison
{
    internal class Difference
    {
        public IReadOnlyCollection<Line> Before { get; set; }
        public IReadOnlyCollection<Line> Expected { get; set; }
        public IReadOnlyCollection<Line> Actual { get; set; }

        public IEnumerable<string> GetLineDifferences(string productName)
        {
            var startingLine = Expected.FirstOrDefault()?.LineNumber ?? Actual.FirstOrDefault()?.LineNumber;
            var endingLine = Expected.LastOrDefault()?.LineNumber ?? Actual.LastOrDefault()?.LineNumber;

            yield return $@"Lines {startingLine}..{endingLine} (vcdb output vs ExpectedOutput.{productName}.sql)";

            foreach (var contextLine in Before.TakeLast(3))
            {
                yield return $"  {contextLine.Content}";
            }
            foreach (var exepectedLine in Expected)
            {
                yield return $"- {exepectedLine.Content}";
            }
            foreach (var actualLine in Actual)
            {
                yield return $"+ {actualLine.Content}";
            }
        }
    }
}
