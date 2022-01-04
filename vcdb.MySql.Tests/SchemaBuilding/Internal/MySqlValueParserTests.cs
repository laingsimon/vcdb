using NUnit.Framework;
using vcdb.MySql.SchemaBuilding.Internal;

namespace vcdb.MySql.Tests.SchemaBuilding.Internal
{
    [TestFixture]
    public class MySqlValueParserTests
    {
        [TestCase("b'0'", 0)]
        [TestCase("b'1'", 1)]
        [TestCase("b'01'", 1)]
        [TestCase("B'0'", 0)]
        [TestCase("B'1'", 1)]
        [TestCase("B'01'", 1)]
        [TestCase("0b00", 0)]
        [TestCase("0b01", 1)]
        public void ParseValue_GivenValidByteValue_ShouldParseCorrectly(string definition, byte expectedValue)
        {
            var parser = new MySqlValueParser();

            var result = parser.ParseValue(definition);

            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [TestCase("x'0'", 0x0)]
        [TestCase("x'1'", 0x1)]
        [TestCase("x'01'", 0x01)]
        [TestCase("X'0'", 0x0)]
        [TestCase("X'1'", 0x1)]
        [TestCase("X'01'", 0x01)]
        [TestCase("0xFF", 0xFF)]
        [TestCase("0xFF", 0xFF)]
        public void ParseValue_GivenValidHexadecimalValue_ShouldParseCorrectly(string definition, byte expectedValue)
        {
            var parser = new MySqlValueParser();

            var result = parser.ParseValue(definition);

            Assert.That(result, Is.EqualTo(expectedValue));
        }
    }
}
