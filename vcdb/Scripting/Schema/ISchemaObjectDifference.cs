namespace vcdb.Scripting.Schema
{
    public interface ISchemaObjectDifference
    {
        bool ObjectAdded { get; }
        bool ObjectDeleted { get; }
        ObjectName ObjectRenamedTo { get; }
        ObjectName CurrentName { get; }
        ObjectName RequiredName { get; }
    }
}
