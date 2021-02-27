namespace vcdb.MySql.SchemaBuilding
{
    public interface IMySqlValueParser
    {
        object ParseDefault(string stringDefinition);
    }
}