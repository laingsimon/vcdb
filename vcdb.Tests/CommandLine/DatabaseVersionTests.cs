using NUnit.Framework;
using System;
using vcdb.CommandLine;

namespace vcdb.Tests.CommandLine
{
    [TestFixture]
    public class DatabaseVersionTests
    {
        [TestCase("SqlServer", "SqlServer", null)]
        [TestCase("SqlServer^", "SqlServer", null)]
        [TestCase("SqlServer^abcd", "SqlServer", "abcd")]
        [TestCase("SqlServer^abc def", "SqlServer", "abc def")]
        public void Parse_WithValidInput_ShouldParseCorrectSections(string input, string expectedProductName, string expectedMinimumVersion)
        {
            var databaseVersion = DatabaseVersion.Parse(input);

            Assert.That(databaseVersion.ProductName, Is.EqualTo(expectedProductName));
            Assert.That(databaseVersion.MinimumCompatibilityVersion, Is.EqualTo(expectedMinimumVersion));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        [TestCase("^abcd")]
        [TestCase("^")]
        [TestCase("^^")]
        [TestCase("SqlServer^abcd^efgh")]
        public void Parse_WithInvalidInput_ShouldThrow(string input)
        {
            Assert.That(
                () => DatabaseVersion.Parse(input),
                Throws.TypeOf<FormatException>());
        }
    }
}
