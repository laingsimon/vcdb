namespace vcdb.Scripting.ExecutionPlan
{
    public interface IScriptTaskDependencyRequiringBuilder
    {
        IScriptTaskDependencyActionBuilder Columns(params TableComponentName[] columns);
        IScriptTaskDependencyActionBuilder ForeignKeyReferencing(ObjectName table);
        IScriptTaskDependencyActionBuilder Index(TableComponentName index);
        IScriptTaskDependencyActionBuilder PrimaryKeyOn(ObjectName table);
        IScriptTaskDependencyActionBuilder Schema(string name);
        IScriptTaskDependencyActionBuilder Table(ObjectName table);
        IScriptTaskDependencyActionBuilder User(string name);
        IScriptTaskDependencyActionBuilder Procedure(ObjectName procedure);
        IScriptTaskDependencyActionBuilder CheckConstraintOn(params TableComponentName[] columns);
        IScriptTaskDependencyActionBuilder ForeignKeyOn(params TableComponentName[] columns);
    }
}