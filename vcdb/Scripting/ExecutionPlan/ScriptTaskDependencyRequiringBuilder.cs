using System;
using System.Collections.Generic;
using System.Linq;

namespace vcdb.Scripting.ExecutionPlan
{
    public class ScriptTaskDependencyRequiringBuilder : IScriptTaskDependencyRequiringBuilder
    {
        private readonly IScriptTask task;

        public ScriptTaskDependencyRequiringBuilder(IScriptTask task)
        {
            this.task = task;
        }

        public IScriptTaskDependencyActionBuilder Columns(params TableComponentName[] columns)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(
                task,
                action => action.GetColumnDependencies(columns, col => new ScriptTaskDependency(action, column: col)));
        }

        public IScriptTaskDependencyActionBuilder Table(ObjectName table)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(task, action => new ScriptTaskDependency(action, table: table));
        }

        public IScriptTaskDependencyActionBuilder PrimaryKeyOn(ObjectName table)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(task, action => new ScriptTaskDependency(action, primaryKeyOn: table));
        }

        public IScriptTaskDependencyActionBuilder ForeignKeyReferencing(ObjectName table)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(task, action => new ScriptTaskDependency(action, foreignKeyReferencing: table));
        }

        public IScriptTaskDependencyActionBuilder Index(TableComponentName index)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(task, action => new ScriptTaskDependency(action, index: index));
        }

        public IScriptTaskDependencyActionBuilder Schema(string name)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(task, action => new ScriptTaskDependency(action, schema: name));
        }

        public IScriptTaskDependencyActionBuilder User(string name)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(task, action => new ScriptTaskDependency(action, user: name));
        }

        public IScriptTaskDependencyActionBuilder Procedure(ObjectName name)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(task, action => new ScriptTaskDependency(action, procedure: name));
        }

        public IScriptTaskDependencyActionBuilder CheckConstraintOn(params TableComponentName[] columns)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(
                task,
                action => action.GetColumnDependencies(columns, col => new ScriptTaskDependency(action, checkConstraintOn: col)));
        }

        public IScriptTaskDependencyActionBuilder ForeignKeyOn(params TableComponentName[] columns)
        {
            return new ScriptTaskRequiredDependencyActionBuilder(
                task,
                action => action.GetColumnDependencies(columns, col => new ScriptTaskDependency(action, foreignKeyOn: col)));
        }

        public class ScriptTaskRequiredDependencyActionBuilder : IScriptTaskDependencyActionBuilder
        {
            private readonly IScriptTask task;
            private readonly Func<DependencyAction, IEnumerable<ScriptTaskDependency>> getDependency;

            public ScriptTaskRequiredDependencyActionBuilder(
                IScriptTask task,
                Func<DependencyAction, ScriptTaskDependency> getDependency)
                : this(task, action => new[] { getDependency(action) })
            { }

            public ScriptTaskRequiredDependencyActionBuilder(
                IScriptTask task,
                Func<DependencyAction, IEnumerable<ScriptTaskDependency>> getDependency)
            {
                this.task = task;
                this.getDependency = getDependency;
            }

            public IScriptTask ToBeDropped()
            {
                return getDependency(DependencyAction.Drop).Aggregate(
                    task,
                    (task, dependency) => task.Requiring(dependency));

            }

            public IScriptTask ToBeCreatedOrAltered()
            {
                return getDependency(DependencyAction.CreateOrAlter).Aggregate(
                    task,
                    (task, dependency) => task.Requiring(dependency));
            }
        }
    }
}
