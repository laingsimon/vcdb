namespace vcdb.SqlServer.SchemaBuilding
{
    public interface ISqlColumnNamedObject
    {
        string Prefix { get; }
        string ColumnName { get; }
        string Name { get; }
        int ObjectId { get; }
    }
}
