using System.Text.RegularExpressions;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerProgrammabilityHelper : ISqlServerProgrammabilityHelper
    {
        public string ChangeProcedureInstructionTo(string definition, string requiredInstruction)
        {
            return Regex.Replace(
                definition,
                @"(?<instruction>(CREATE\s+OR\s+ALTER)|(CREATE)|(ALTER))\s+(?:PROC|PROCEDURE)\s+",
                (match) =>
                {
                    var wholeGroup = match.Value;
                    var instructionGroup = match.Groups["instruction"];

                    return requiredInstruction + wholeGroup.Substring(instructionGroup.Length);
                },
                RegexOptions.IgnoreCase);
        }
    }
}
