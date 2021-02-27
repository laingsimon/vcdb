namespace vcdb.MySql.SchemaBuilding
{
    public interface IDataTypeParser
    {
        string GetDataType(string type, string collation);
        bool IsNationalCharacterColumn(string type, string collation);
    }
}