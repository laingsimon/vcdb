namespace vcdb.MySql.SchemaBuilding.Internal
{
    public interface IMySqlValueParser
    {
        object ParseDefault(string stringDefinition);
        object ParseValue(string definition);
    }
}