﻿using Newtonsoft.Json;
using NUnit.Framework;

namespace vcdb.Tests
{
    [TestFixture]
    public class TableNameTests
    {
        [TestCase("dbo.Table", "dbo", "Table")]
        [TestCase("[dbo].Table", "dbo", "Table")]
        [TestCase("[dbo].[Table]", "dbo", "Table")]
        public void ConvertFrom_ShouldDeserialiseCorrectly(string input, string expectedSchemaName, string expectedTableName)
        {
            var json = $"\"{input}\"";

            var result = JsonConvert.DeserializeObject<TableName>(json);

            Assert.That(result.Schema, Is.EqualTo(expectedSchemaName));
            Assert.That(result.Table, Is.EqualTo(expectedTableName));
        }

        [Test]
        public void ConvertTo_ShouldSerialiseCorrectly()
        {
            var input = new TableName
            {
                Schema = "dbo",
                Table = "Table"
            };

            var json = JsonConvert.SerializeObject(input);

            Assert.That(json, Is.EqualTo("\"[dbo].[Table]\""));
        }
    }
}