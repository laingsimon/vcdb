using Newtonsoft.Json;
using NUnit.Framework;

namespace vcdb.Tests
{
    [TestFixture]
    public class ObjectNameTests
    {
        [TestCase("dbo.Table", "dbo", "Table")]
        [TestCase("[dbo].Table", "dbo", "Table")]
        [TestCase("[dbo].[Table]", "dbo", "Table")]
        public void ConvertFrom_ShouldDeserialiseCorrectly(string input, string expectedSchemaName, string expectedTableName)
        {
            ObjectName.Converter = new ObjectNameConverter(".", "[", "]");

            var json = $"\"{input}\"";

            var result = JsonConvert.DeserializeObject<ObjectName>(json);

            Assert.That(result.Schema, Is.EqualTo(expectedSchemaName));
            Assert.That(result.Name, Is.EqualTo(expectedTableName));
        }

        [Test]
        public void ConvertTo_ShouldSerialiseCorrectly()
        {
            var converter = new ObjectNameConverter(".", "[", "]");

            var input = new ObjectName(converter)
            {
                Schema = "dbo",
                Name = "Table"
            };

            var json = JsonConvert.SerializeObject(input);

            Assert.That(json, Is.EqualTo("\"[dbo].[Table]\""));
        }

        [Test]
        public void GetHashCode_WhenTheDefaultObjectNameConverterChanges_ShouldNotChangeHashCode()
        {
            ObjectName.Converter = new ObjectNameConverter(".", "[", "]");
            var name = new ObjectName { Schema = "dbo", Name = "Table" };
            var firstHashCode = name.GetHashCode();

            ObjectName.Converter = new ObjectNameConverter(".", "`", "`");

            Assert.That(name.GetHashCode(), Is.EqualTo(firstHashCode));
        }

        [Test]
        public void ObjectName_WhenTheDefaultConverterChanges_ShouldDeserialiseWithTheNewConverter()
        {
            ObjectName.Converter = new ObjectNameConverter(".", "[", "]");
            var sqlServerStyleResult = JsonConvert.DeserializeObject<ObjectName>("\"[dbo].[Table]\"");
            Assert.That(sqlServerStyleResult.Schema, Is.EqualTo("dbo"));
            Assert.That(sqlServerStyleResult.Name, Is.EqualTo("Table"));

            ObjectName.Converter = new ObjectNameConverter(".", "`", "`");
            var mysqlStyleResult = JsonConvert.DeserializeObject<ObjectName>("\"`dbo`.`Table`\"");

            Assert.That(mysqlStyleResult.Schema, Is.EqualTo("dbo"));
            Assert.That(mysqlStyleResult.Name, Is.EqualTo("Table"));
        }

        [Test]
        public void GetHashCode_WhenDeserialisedWithDifferentConverters_ShouldReturnTheSameHashCode()
        {
            ObjectName.Converter = new ObjectNameConverter(".", "[", "]");
            var sqlServerStyleResult = JsonConvert.DeserializeObject<ObjectName>("\"[dbo].[Table]\"");
            ObjectName.Converter = new ObjectNameConverter(".", "`", "`");
            var mysqlStyleResult = JsonConvert.DeserializeObject<ObjectName>("\"`dbo`.`Table`\"");

            Assert.That(mysqlStyleResult.GetHashCode(), Is.EqualTo(sqlServerStyleResult.GetHashCode()));
        }
    }
}
