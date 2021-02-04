using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Schema
{
    public interface ISchemaScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(
            IReadOnlyCollection<SchemaDifference> schemaDifferences,
            IReadOnlyCollection<ISchemaObjectDifference> schemaObjectDifferences);
    }
}