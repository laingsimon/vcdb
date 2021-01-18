using NUnit.Framework;
using System.Linq;
using TestFramework.Comparison;

namespace TestFramework.Tests.Comparison
{
    [TestFixture]
    public class RegexDifferenceFilterTests
    {
        private const string RegexDelimiter = "/";

        [Test]
        public void FilterDifferences_WhenGivenNoDifferences_ShouldReturnEmptyCollection()
        {
            var filter = new RegexDifferenceFilter();

            var result = filter.FilterDifferences(new Difference[0]).ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithOnlyAddedLines_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line was added") },
                Expected = new Line[0],
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference }));
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithOnlyRemovedLines_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new Line[0],
                Expected = new[] { new Line(0, "this line was removed") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference }));
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithoutRegexDelimiter_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line was generated") },
                Expected = new[] { new Line(0, "this line was generated") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference }));
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithSingleRegexDelimiter_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line was generated") },
                Expected = new[] { new Line(0, $"this line was {RegexDelimiter}generated") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference }));
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithMoreExpectedLines_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line was generated") },
                Expected = new[] { new Line(0, $"this line was generated"), new Line(1, "another line") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference }));
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithMoreActualLines_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line was generated"), new Line(1, "another line") },
                Expected = new[] { new Line(0, $"this line was generated") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference }));
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithSingleMatchingRegex_ShouldExcludeDifference()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line was generated") },
                Expected = new[] { new Line(0, $"this line {RegexDelimiter}[asw]{{3}}{RegexDelimiter} generated") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithSingleMismatchingRegex_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line vcdb generated") },
                Expected = new[] { new Line(0, $"this line {RegexDelimiter}[asw]{{3}}{RegexDelimiter} generated") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference }));
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithMultipleMatchingRegexes_ShouldExcludeDifference()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line was generated") },
                Expected = new[] { new Line(0, $"this {RegexDelimiter}[a-z]{{4}}{RegexDelimiter} {RegexDelimiter}[asw]{{3}}{RegexDelimiter} generated") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithMultipleRegexesAndOneDoesntMatch_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this line vcdb generated") },
                Expected = new[] { new Line(0, $"this {RegexDelimiter}[a-z]{{4}}{RegexDelimiter} {RegexDelimiter}[asw]{{3}}{RegexDelimiter} generated") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference }));
        }

        [Test]
        public void FilterDifferences_WhenGivenDifferenceWithRegexUnsafeCharactersOutsideRegexDelimiter_ShouldExcludeDifference()
        {
            var filter = new RegexDifferenceFilter();
            var difference = new Difference
            {
                Actual = new[] { new Line(0, "this [line] was generated{3}") },
                Expected = new[] { new Line(0, $"this [line] {RegexDelimiter}[asw]{{3}}{RegexDelimiter} generated{{3}}") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference }).ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FilterDifferences_WhenGivenMultipleMismatchingDifferences_ShouldReturnSameDifferences()
        {
            var filter = new RegexDifferenceFilter();
            var difference1 = new Difference
            {
                Actual = new[] { new Line(0, "this [line] vcdb generated") },
                Expected = new[] { new Line(0, $"this [line] {RegexDelimiter}[asw]{{3}}{RegexDelimiter} generated") },
                Before = new Line[0]
            };
            var difference2 = new Difference
            {
                Actual = new[] { new Line(0, "this [line] was generated") },
                Expected = new[] { new Line(0, $"this [line] WAS generated") },
                Before = new Line[0]
            };

            var result = filter.FilterDifferences(new[] { difference1, difference2 }).ToArray();

            Assert.That(result, Is.EqualTo(new[] { difference1, difference2 }));
        }
    }
}
