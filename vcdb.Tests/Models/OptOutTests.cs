using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using vcdb.Models;
using vcdb.Output;

namespace vcdb.Tests.Models
{
    [TestFixture]
    public class OptOutTests
    {
        [Test]
        public void OptOutTrue_ShouldBeEqualToEquivalent()
        {
            Assert.That(OptOut.True.Equals(true), Is.True);
            Assert.That(OptOut.True.Equals(OptOut.True), Is.True);
            Assert.That(OptOut.True.Equals((object)OptOut.True), Is.True);
            Assert.That(OptOut.True.Equals((object)null), Is.True);
            Assert.That(OptOut.True.Equals((OptOut)null), Is.True);
            Assert.That(OptOut.True == true, Is.True);
            Assert.That(true == OptOut.True, Is.True);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.That(OptOut.True == OptOut.True, Is.True);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Test]
        public void OptOutTrue_ShouldBeUnequalToOpposite()
        {
            Assert.That(OptOut.True.Equals(false), Is.False);
            Assert.That(OptOut.True.Equals(OptOut.False), Is.False);
            Assert.That(OptOut.True.Equals((object)OptOut.False), Is.False);
            Assert.That(OptOut.True.Equals(new object()), Is.False);
            Assert.That(OptOut.True != false, Is.True);
            Assert.That(false != OptOut.True, Is.True);
            Assert.That(OptOut.True != OptOut.False, Is.True);
        }

        [Test]
        public void OptOutFalse_ShouldBeEqualToEquivalent()
        {
            Assert.That(OptOut.False.Equals(false), Is.True);
            Assert.That(OptOut.False.Equals(OptOut.False), Is.True);
            Assert.That(OptOut.False.Equals((object)OptOut.False), Is.True);
            Assert.That(OptOut.False.Equals((object)null), Is.False);
            Assert.That(OptOut.False.Equals((OptOut)null), Is.False);
            Assert.That(OptOut.False == false, Is.True);
            Assert.That(false == OptOut.False, Is.True);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.That(OptOut.False == OptOut.False, Is.True);
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Test]
        public void OptOutFalse_ShouldBeUnequalToOpposite()
        {
            Assert.That(OptOut.False.Equals(true), Is.False);
            Assert.That(OptOut.False.Equals(OptOut.True), Is.False);
            Assert.That(OptOut.False.Equals((object)OptOut.True), Is.False);
            Assert.That(OptOut.False.Equals(new object()), Is.False);
            Assert.That(OptOut.False != true, Is.True);
            Assert.That(true != OptOut.False, Is.True);
            Assert.That(OptOut.False != OptOut.True, Is.True);
        }

        [TestCaseSource(nameof(FromTestCases))]
        public void From_ShouldReturnCorrectOptOut(bool? input, OptOut expectedOutput)
        {
            var result = OptOut.From(input);

            Assert.That(result, Is.SameAs(expectedOutput));
        }

        public static IEnumerable<TestCaseData> FromTestCases
        {
            get
            {
                yield return new TestCaseData(null, OptOut.Default);
                yield return new TestCaseData(true, OptOut.True);
                yield return new TestCaseData(false, OptOut.False);
            }
        }

        [Test]
        public void Converter_ShouldSerialiseCorrectly()
        {
            var serialiser = new JsonSerializer();
            var writer = new StringWriter();
            var instance = new TestBedOptOut
            {
                Default = OptOut.Default,
                True = OptOut.True,
                False = OptOut.False
            };

            using (var jsonWriter = new JsonTextWriter(writer))
                serialiser.Serialize(jsonWriter, instance);

            var asBools = JsonConvert.DeserializeObject<TestBedBool>(writer.GetStringBuilder().ToString());
            Assert.That(asBools.Default, Is.True);
            Assert.That(asBools.True, Is.True);
            Assert.That(asBools.False, Is.False);
        }

        [Test]
        public void Converter_ShouldDeserialiseCorrectly()
        {
            var serialiser = new JsonSerializer();
            var instance = new TestBedBool
            {
                Default = null,
                True = true,
                False = false
            };
            var json = JsonConvert.SerializeObject(instance);

            using (var jsonReader = new JsonTextReader(new StringReader(json)))
            {
                var asOptOut = serialiser.Deserialize<TestBedOptOut>(jsonReader);

                Assert.That(asOptOut.Default, Is.SameAs(OptOut.Default));
                Assert.That(asOptOut.True, Is.SameAs(OptOut.True));
                Assert.That(asOptOut.False, Is.SameAs(OptOut.False));
            }
        }

        [Test]
        public void ContractResolver_ShouldSerialiseCorrectly()
        {
            var serialiser = new JsonSerializer
            {
                ContractResolver = new MultipleJsonContractResolver(new OptOut.OptOutContractResover())
            };
            var writer = new StringWriter();
            var instance = new TestBedOptOut
            {
                Default = OptOut.Default,
                True = OptOut.True,
                False = OptOut.False
            };

            using (var jsonWriter = new JsonTextWriter(writer))
                serialiser.Serialize(jsonWriter, instance);

            var json = writer.GetStringBuilder().ToString();
            Assert.That(json, Does.Contain("\"False\":false"));
            Assert.That(json, Does.Not.Contain("\"True\""));
            Assert.That(json, Does.Not.Contain("\"Default\""));
        }

        [Test]
        public void ContractResolver_ShouldDeserialiseCorrectly()
        {
            var serialiser = new JsonSerializer
            {
                ContractResolver = new MultipleJsonContractResolver(new OptOut.OptOutContractResover())
            };
            var instance = new TestBedBool
            {
                Default = null,
                True = true,
                False = false
            };
            var json = JsonConvert.SerializeObject(instance);

            using (var jsonReader = new JsonTextReader(new StringReader(json)))
            {
                var asOptOut = serialiser.Deserialize<TestBedOptOut>(jsonReader);

                Assert.That(asOptOut.Default, Is.SameAs(OptOut.Default));
                Assert.That(asOptOut.True, Is.SameAs(OptOut.True));
                Assert.That(asOptOut.False, Is.SameAs(OptOut.False));
            }
        }

        [Test]
        public void ContractResolver_ShouldDeserialiseFromAbsentPropertiesCorrectly()
        {
            var serialiser = new JsonSerializer
            {
                ContractResolver = new MultipleJsonContractResolver(new OptOut.OptOutContractResover())
            };
            var json = @"{ ""False"": false }";

            using (var jsonReader = new JsonTextReader(new StringReader(json)))
            {
                var asOptOut = serialiser.Deserialize<TestBedOptOut>(jsonReader);

                Assert.That(OptOut.Default.Equals(asOptOut.Default), Is.True);
                Assert.That(OptOut.True.Equals(asOptOut.True), Is.True);
                Assert.That(OptOut.False.Equals(asOptOut.False), Is.True);
            }
        }

        private class TestBedOptOut
        {
            public OptOut True { get; set; }
            public OptOut False { get; set; }
            public OptOut Default { get; set; }
        }

        private class TestBedBool
        {
            public bool? True { get; set; }
            public bool? False { get; set; }
            public bool? Default { get; set; }
        }
    }
}
