using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.Output;

namespace vcdb.Scripting.ExecutionPlan
{
    public static class ScriptTaskExtensions
    {
        /// <summary>
        /// Does this task require the other task to be executed first?
        /// </summary>
        /// <param name="task"></param>
        /// <param name="otherTask"></param>
        /// <returns></returns>
        public static bool RequiresTask(this IScriptTask task, IScriptTask otherTask)
        {
            return task.Requires.Any(dependency =>
            {
                return otherTask.ResultsIn.Any(resultingDependency => resultingDependency.Equals(dependency));
            });
        }

        /// <summary>
        /// Express that this script requires another to execute first
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static IScriptTaskDependencyRequiringBuilder Requiring(this IOutputable script)
        {
            return new ScriptTaskDependencyRequiringBuilder(
                     script.AsTask());
        }

        /// <summary>
        /// Express that this script drops an object that may form a pre-requisite of another script
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static IScriptTaskDependencyResultsInBuilder Drops(this IOutputable script)
        {
            return new ScriptTaskDependencyResultsInBuilder(
               script.AsTask(),
               action: DependencyAction.Drop);
        }

        /// <summary>
        /// Express that this script creates, alters or renames an object that may form a pre-requisite of another script
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static IScriptTaskDependencyResultsInBuilder CreatesOrAlters(this IOutputable script)
        {
            return new ScriptTaskDependencyResultsInBuilder(
                  script.AsTask(),
                  action: DependencyAction.CreateOrAlter);
        }

        public static IScriptTask AsTask(this IOutputable script)
        {
            return (script as ScriptTask) ?? new ScriptTask(script);
        }

        public static TableComponentName Component(this ObjectName table, string component)
        {
            return new TableComponentName(table, component);
        }

        public static TableComponentName[] Components(this ObjectName table, params string[] components)
        {
            return table.Components((IEnumerable<string>)components);
        }

        public static TableComponentName[] Components(this ObjectName table, IEnumerable<string> components)
        {
            return components.Select(component => new TableComponentName(table, component)).ToArray();
        }

        public static IEnumerable<ScriptTaskDependency> GetColumnDependencies(
            this DependencyAction action,
            IEnumerable<TableComponentName> columns,
            Func<TableComponentName, ScriptTaskDependency> createColumnDependency)
        {
            var tableActions = action == DependencyAction.CreateOrAlter
                ? columns.Select(c => c.Table).Distinct().Select(table => new ScriptTaskDependency(action, table: table))
                : Enumerable.Empty<ScriptTaskDependency>();
            var columnActions = columns
                .Where(name => !string.IsNullOrEmpty(name.Component))
                .Select(createColumnDependency);

            return tableActions.Concat(columnActions);
        }
    }
}
