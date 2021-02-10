namespace vcdb.Scripting.ExecutionPlan
{
    public interface IScriptTaskDependencyResultsInBuilder
    {
        IScriptTask Columns(params TableComponentName[] columns);
        IScriptTask ForeignKeyReferencing(ObjectName table);
        IScriptTask Index(TableComponentName index);
        IScriptTask PrimaryKeyOn(ObjectName table);
        IScriptTask Schema(string name);
        IScriptTask Table(ObjectName table);
        IScriptTask User(string name);
        IScriptTask Procedure(ObjectName procedure);
        IScriptTask CheckConstraintOn(params TableComponentName[] columns);
        IScriptTask ForeignKeyOn(params TableComponentName[] columns);
    }
}