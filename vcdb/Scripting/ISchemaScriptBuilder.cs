using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting
{
    public interface ISchemaScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<SchemaDifference> schemaDifferences);
    }
}