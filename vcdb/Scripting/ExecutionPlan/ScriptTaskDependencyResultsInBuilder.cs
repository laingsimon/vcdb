using System.Linq;

namespace vcdb.Scripting.ExecutionPlan
{
    public class ScriptTaskDependencyResultsInBuilder : IScriptTaskDependencyResultsInBuilder
    {
        private readonly IScriptTask task;
        private readonly DependencyAction action;

        public ScriptTaskDependencyResultsInBuilder(
            IScriptTask task,
            DependencyAction action)
        {
            this.task = task;
            this.action = action;
        }

        public IScriptTask Columns(params TableComponentName[] columns)
        {
            var dependencies = action.GetColumnDependencies(columns, col => new ScriptTaskDependency(action, column: col));
            return dependencies.Aggregate(
                task,
                (task, dependency) => task.ResultingIn(dependency));
        }

        public IScriptTask Table(ObjectName table)
        {
            return task.ResultingIn(new ScriptTaskDependency(action, table: table));
        }

        public IScriptTask PrimaryKeyOn(ObjectName table)
        {
            return task.ResultingIn(new ScriptTaskDependency(action, primaryKeyOn: table));
        }

        public IScriptTask ForeignKeyReferencing(ObjectName table)
        {
            return task.ResultingIn(new ScriptTaskDependency(action, foreignKeyReferencing: table));
        }

        public IScriptTask Index(TableComponentName index)
        {
            return task.ResultingIn(new ScriptTaskDependency(action, index: index));
        }

        public IScriptTask Schema(string name)
        {
            return task.ResultingIn(new ScriptTaskDependency(action, schema: name));
        }

        public IScriptTask User(string name)
        {
            return task.ResultingIn(new ScriptTaskDependency(action, user: name));
        }

        public IScriptTask Procedure(ObjectName procedure)
        {
            return task.ResultingIn(new ScriptTaskDependency(action, procedure: procedure));
        }

        public IScriptTask CheckConstraintOn(params TableComponentName[] columns)
        {
            var dependencies = action.GetColumnDependencies(columns, col => new ScriptTaskDependency(action, checkConstraintOn: col));

            return dependencies.Aggregate(
                task,
                (task, dependency) => task.ResultingIn(dependency));
        }

        public IScriptTask ForeignKeyOn(params TableComponentName[] columns)
        {
            var dependencies = action.GetColumnDependencies(columns, col => new ScriptTaskDependency(action, foreignKeyOn: col));

            return dependencies.Aggregate(
                task,
                (task, dependency) => task.ResultingIn(dependency));
        }
    }
}
