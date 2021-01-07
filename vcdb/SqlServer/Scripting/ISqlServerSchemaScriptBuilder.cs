using System.Collections.Generic;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer.Scripting
{
    public interface ISqlServerSchemaScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<SchemaDifference> schemaDifferences, bool beforeTableRenamesAndTransfers);
    }
}
