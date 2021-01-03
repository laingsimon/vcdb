﻿using NUnit.Framework;
using vcdb.SqlServer;

namespace vcdb.Tests.SqlServer
{
    [TestFixture]
    public class SqlObjectNameHelperTests
    {
        [Test]
        public void GetAutomaticConstraintName_ShouldProduceCorrectDefaultName()
        {
            var helper = new SqlObjectNameHelper();

            var result = helper.GetAutomaticConstraintName(
                "DF",
                "Table",
                "Column",
                901578250);

            Assert.That(result, Is.EqualTo("DF__Table__Column__35BCFE0A"));
        }
    }
}
