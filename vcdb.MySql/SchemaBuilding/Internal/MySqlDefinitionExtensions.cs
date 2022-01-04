using System.Text.RegularExpressions;

namespace vcdb.MySql.SchemaBuilding.Internal
{
    public static class MySqlDefinitionExtensions
    {
        public static string UnwrapDefinition(this string definition)
        {
            if (string.IsNullOrEmpty(definition))
                return null;

            while (definition.StartsWith("(") && definition.EndsWith(")"))
                definition = definition.Substring(1, definition.Length - 2);

            return Regex.Replace(
                definition,
                @"(\(\d+(?:\.\d+)?\))|(\(\'.+?\'\))",
                match => match.Value.UnwrapDefinition());
        }
    }
}
