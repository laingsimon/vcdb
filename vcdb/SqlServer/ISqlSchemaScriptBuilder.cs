using System.Collections.Generic;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer
{
    public interface ISchemaScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<SchemaDifference> schemaDifferences);
    }
}