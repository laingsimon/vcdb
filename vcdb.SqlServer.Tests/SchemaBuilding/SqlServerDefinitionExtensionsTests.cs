using NUnit.Framework;
using vcdb.SqlServer.SchemaBuilding;

namespace vcdb.SqlServer.Tests.SchemaBuilding
{
    [TestFixture]
    public class SqlServerDefinitionExtensionsTests
    {
        [TestCase("aaa", "aaa")]
        [TestCase("aaa()", "aaa()")]
        [TestCase("(aaa())", "aaa()")]
        [TestCase("(0)", "0")]
        [TestCase("((0))", "0")]
        [TestCase("('0')", "'0'")]
        [TestCase("([A] * (5))", "[A] * 5")]
        [TestCase("([A] + ('5'))", "[A] + '5'")]
        [TestCase("([A] * (5.5))", "[A] * 5.5")]
        public void UnwrapDefinition_ShouldReturnCorrectValues(string definition, string expectedResult)
        {
            var result = definition.UnwrapDefinition();

            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}
