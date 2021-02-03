using System.Text.RegularExpressions;
using vcdb.Scripting.Programmability;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerProcedureDefinitionComparer : IProcedureDefinitionComparer
    {
        public bool Equals(NamedItem<ObjectName, string> currentDefinition, NamedItem<ObjectName, string> requiredDefinition)
        {
            var currentNormalisedDefinition = NormaliseDefinition(currentDefinition);
            var requiredNormalisedDefinition = NormaliseDefinition(requiredDefinition);

            return currentNormalisedDefinition == requiredNormalisedDefinition;
        }

        public int GetHashCode(NamedItem<ObjectName, string> obj)
        {
            return NormaliseDefinition(obj).GetHashCode()
                ^ obj.Key.GetHashCode();
        }

        private static string NormaliseDefinition(NamedItem<ObjectName, string> definitionReference)
        {
            if (string.IsNullOrEmpty(definitionReference.Value))
                return definitionReference.Value;

            var definition = definitionReference.Value;
            var name = definitionReference.Key;
            var createOrAlterStatement = "CREATE OR ALTER PROCEDURE";
            var createStatementNormalised = Regex.Replace(definition.Trim(), @"(CREATE\s+OR\s+ALTER|CREATE|ALTER)\s+(PROCEDURE|PROC)", createOrAlterStatement, RegexOptions.IgnoreCase);
            var nameNormalised = Regex.Replace(createStatementNormalised, $@"{createOrAlterStatement}\s+\[?{Regex.Escape(name.Schema)}\]?\.\[{Regex.Escape(name.Name)}\]", $"{createOrAlterStatement} [_NORMALISED_NAME_]");
            return nameNormalised;
        }
    }
}
