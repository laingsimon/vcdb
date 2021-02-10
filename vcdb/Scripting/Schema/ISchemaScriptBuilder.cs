using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.Schema
{
    public interface ISchemaScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(
            IReadOnlyCollection<SchemaDifference> schemaDifferences,
            IReadOnlyCollection<ISchemaObjectDifference> schemaObjectDifferences);
    }
}