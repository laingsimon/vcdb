using NUnit.Framework;
using vcdb.SqlServer.Scripting;

namespace vcdb.SqlServer.Tests.Scripting
{
    [TestFixture]
    public class SqlServerProgrammabilityHelperTests
    {
        [TestCase(@"CREATE OR ALTER PROC [dbo].[something]", "CREATE", "CREATE PROC [dbo].[something]")]
        [TestCase(@"CREATE OR ALTER PROCEDURE [dbo].[something]", "CREATE", "CREATE PROCEDURE [dbo].[something]")]
        [TestCase(@"create or alter proc [dbo].[something]", "CREATE", "CREATE proc [dbo].[something]")]
        [TestCase(@"create or alter procedure [dbo].[something]", "CREATE", "CREATE procedure [dbo].[something]")]
        [TestCase(@"CREATE   OR   ALTER   PROC   [dbo].[something]", "CREATE", "CREATE   PROC   [dbo].[something]")]
        [TestCase(@"CREATE   OR   ALTER   PROCEDURE   [dbo].[something]", "CREATE", "CREATE   PROCEDURE   [dbo].[something]")]
        [TestCase(@"create   or   alter   proc   [dbo].[something]", "CREATE", "CREATE   proc   [dbo].[something]")]
        [TestCase(@"create   or   alter   procedure   [dbo].[something]", "CREATE", "CREATE   procedure   [dbo].[something]")]
        [TestCase(@"CREATE PROC [dbo].[something]", "CREATE", "CREATE PROC [dbo].[something]")]
        [TestCase(@"CREATE PROCEDURE [dbo].[something]", "CREATE", "CREATE PROCEDURE [dbo].[something]")]
        [TestCase(@"create proc [dbo].[something]", "CREATE", "CREATE proc [dbo].[something]")]
        [TestCase(@"create procedure [dbo].[something]", "CREATE", "CREATE procedure [dbo].[something]")]
        [TestCase(@"CREATE   PROC   [dbo].[something]", "CREATE", "CREATE   PROC   [dbo].[something]")]
        [TestCase(@"CREATE   PROCEDURE   [dbo].[something]", "CREATE", "CREATE   PROCEDURE   [dbo].[something]")]
        [TestCase(@"create   proc   [dbo].[something]", "CREATE", "CREATE   proc   [dbo].[something]")]
        [TestCase(@"create   procedure   [dbo].[something]", "CREATE", "CREATE   procedure   [dbo].[something]")]
        [TestCase(@"ALTER PROC [dbo].[something]", "CREATE", "CREATE PROC [dbo].[something]")]
        [TestCase(@"ALTER PROCEDURE [dbo].[something]", "CREATE", "CREATE PROCEDURE [dbo].[something]")]
        [TestCase(@"alter proc [dbo].[something]", "CREATE", "CREATE proc [dbo].[something]")]
        [TestCase(@"alter procedure [dbo].[something]", "CREATE", "CREATE procedure [dbo].[something]")]
        [TestCase(@"ALTER   PROC   [dbo].[something]", "CREATE", "CREATE   PROC   [dbo].[something]")]
        [TestCase(@"ALTER   PROCEDURE   [dbo].[something]", "CREATE", "CREATE   PROCEDURE   [dbo].[something]")]
        [TestCase(@"alter   proc   [dbo].[something]", "CREATE", "CREATE   proc   [dbo].[something]")]
        [TestCase(@"alter   procedure   [dbo].[something]", "CREATE", "CREATE   procedure   [dbo].[something]")]
        public void ChangeProcedureInstructionTo_WhenGivenMatchingInput_ShouldChangeInstructionCorrectly(string definition, string newInstruction, string expectedDefinition)
        {
            var helper = new SqlServerProgrammabilityHelper();

            var output = helper.ChangeProcedureInstructionTo(definition, newInstruction);

            Assert.That(output, Is.EqualTo(expectedDefinition));
        }

        [TestCase(@"CREATE OR ALTER PROC[dbo].[something]", "CREATE")]
        [TestCase(@"CREATEORALTER PROC[dbo].[something]", "CREATE")]
        [TestCase(@"CREATE OR ALTER PROCEDURE[dbo].[something]", "CREATE")]
        [TestCase(@"CREATEORALTER PROCEDURE [dbo].[something]", "CREATE")]
        public void ChangeProcedureInstructionTo_WhenGivenMismatchingInput_ShouldNotChangeInputDefinition(string definition, string newInstruction)
        {
            var helper = new SqlServerProgrammabilityHelper();

            var output = helper.ChangeProcedureInstructionTo(definition, newInstruction);

            Assert.That(output, Is.EqualTo(definition));
        }
    }
}
