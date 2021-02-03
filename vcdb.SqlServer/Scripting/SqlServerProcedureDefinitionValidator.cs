using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using vcdb.Models;
using vcdb.Scripting.Programmability;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerProcedureDefinitionValidator : IProcedureDefinitionValidator
    {
        public bool IsRenamedDefinitionOnly(string currentDefinition, string requiredDefinition, ObjectName currentName, ObjectName requiredName)
        {
            if (currentDefinition == null || requiredDefinition == null)
                return false;

            var renamedCurrentDefinition = currentDefinition.Replace(currentName.SqlSafeName(), requiredName.SqlSafeName());
            return renamedCurrentDefinition == requiredDefinition;
        }

        public string NormaliseDefinition(string definition)
        {
            if (string.IsNullOrEmpty(definition))
                return definition;

            return Regex.Replace(definition.Trim(), @"(CREATE\s+OR\s+ALTER|CREATE|ALTER)\s+(PROCEDURE|PROC)", "CREATE OR ALTER PROCEDURE", RegexOptions.IgnoreCase);
        }

        public IEnumerable<string> ValidateDefinition(string definition, NamedItem<ObjectName, ProcedureDetails> procedure)
        {
            return DefintionContainsIncorrectNumberOfCreateOrAlterStatements(definition)
                .Concat(DefintionContainsCreateOrAlterForADifferentProcedure(definition, procedure.Key))
                .Concat(DefinitionContainsAGoStatement(definition))
                .Select(error => $"The procedure definition for {procedure.Key.SqlSafeName()} is invalid: {error}");
        }

        private IEnumerable<string> DefinitionContainsAGoStatement(string definition)
        {
            if (Regex.IsMatch(definition, @"\s+GO\s+"))
                yield return $"GO statements are not permitted";
        }

        private IEnumerable<string> DefintionContainsCreateOrAlterForADifferentProcedure(string definition, ObjectName key)
        {
            return GetCreateOrAlterProcedureNames(definition)
                .Where(procedureName => !procedureName.Equals(key))
                .Select(procedureName => $"The definition contains a CREATE or ALTER statement for a different procedure: {procedureName.SqlSafeName()}");
        }

        private IEnumerable<string> DefintionContainsIncorrectNumberOfCreateOrAlterStatements(string definition)
        {
            var createOrAlterStatements = GetCreateOrAlterProcedureNames(definition).Count();

            if (createOrAlterStatements > 1)
                yield return $"The definition contains multiple ({createOrAlterStatements}) CREATE or ALTER statements";

            if (createOrAlterStatements == 0)
                yield return $"The definition does not contain a CREATE or ALTER statement";
        }

        private IEnumerable<ObjectName> GetCreateOrAlterProcedureNames(string definition)
        {
            var createOrAlterStatements = Regex.Matches(definition, @"(CREATE(\s+OR ALTER)?|ALTER)\s+(PROC|PROCEDURE)\s+(?<procedureName>.+)\s+", RegexOptions.IgnoreCase);
            return createOrAlterStatements.Cast<Match>().Select(match => ObjectName.Parse(match.Groups["procedureName"].Value.Trim()));
        }
    }
}
