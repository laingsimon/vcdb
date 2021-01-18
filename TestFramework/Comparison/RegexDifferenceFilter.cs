using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TestFramework.Comparison
{
    public class RegexDifferenceFilter : IDifferenceFilter
    {
        private const string RegexDelimiter = "/";

        public IEnumerable<Difference> FilterDifferences(IEnumerable<Difference> differences)
        {
            foreach (var difference in differences)
            {
                if (difference.Expected.Count != difference.Actual.Count || difference.Expected.All(line => !line.Content.Contains(RegexDelimiter)))
                {
                    yield return difference;
                    continue;
                }

                if (!LinesMatch(difference))
                {
                    yield return difference;
                }
            }
        }

        private bool LinesMatch(Difference difference)
        {
            var zipped = difference.Expected.Zip(
                difference.Actual,
                (expected, actual) => new { expected, actual });

            foreach (var line in zipped)
            {
                if (!line.expected.Content.Contains(RegexDelimiter))
                {
                    return false; //the lines are different, and cannot be matched with regex
                }

                if (!LineMatches(line.expected.Content, line.actual.Content))
                {
                    return false;
                }
            }

            return true;
        }

        private bool LineMatches(string expected, string actual)
        {
            var matches = Regex.Matches(expected, @"(\/.+?\/)");
            var stringBuilder = new StringBuilder();
            var lastIndex = 0;

            foreach (Match match in matches)
            {
                var beforeMatch = expected.Substring(lastIndex, match.Index - lastIndex);
                stringBuilder.Append(Regex.Escape(beforeMatch));

                var regexPart = match.Value.Substring(RegexDelimiter.Length, match.Value.Length - (RegexDelimiter.Length * 2));
                stringBuilder.Append($"({regexPart})");
                lastIndex = match.Index + match.Length;
            }

            stringBuilder.Append(Regex.Escape(expected.Substring(lastIndex, expected.Length - lastIndex)));
            var expectedAsRegex = stringBuilder.ToString();

            return Regex.IsMatch(actual, expectedAsRegex);
        }
    }
}
