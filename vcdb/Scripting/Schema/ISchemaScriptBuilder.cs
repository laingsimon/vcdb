using System.Collections.Generic;
using vcdb.Output;
using vcdb.Scripting.Programmability;
using vcdb.Scripting.Table;

namespace vcdb.Scripting.Schema
{
    public interface ISchemaScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(
            IReadOnlyCollection<SchemaDifference> schemaDifferences,
            IReadOnlyCollection<TableDifference> tableDifferences,
            IReadOnlyCollection<ProcedureDifference> procedureDifferences);
    }
}