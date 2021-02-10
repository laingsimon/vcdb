namespace vcdb.Scripting.ExecutionPlan
{
    public interface IScriptTaskDependencyActionBuilder
    {
        IScriptTask ToBeDropped();
        IScriptTask ToBeCreatedOrAltered();
    }
}