namespace vcdb.SchemaBuilding
{
    public interface IColumnDefault
    {
        string Name { get; }
        int ObjectId { get; }
        bool IsSystemNamed { get; }
    }
}
