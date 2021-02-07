namespace vcdb.SqlServer.SchemaBuilding
{
    public static class SqlServerDefinitionExtensions
    {
        public static string UnwrapDefinition(this string definition)
        {
            while (definition.StartsWith("(") && definition.EndsWith(")"))
                definition = definition.Substring(1, definition.Length - 2);

            return definition;
        }
    }
}
