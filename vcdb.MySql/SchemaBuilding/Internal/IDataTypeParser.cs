namespace vcdb.MySql.SchemaBuilding.Internal
{
    public interface IDataTypeParser
    {
        string GetDataType(string type, string collation);
        bool IsNationalCharacterColumn(string type, string collation);
    }
}