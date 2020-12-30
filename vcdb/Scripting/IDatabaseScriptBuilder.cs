using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;

namespace vcdb.Scripting
{
    public interface IDatabaseScriptBuilder
    {
        IAsyncEnumerable<SqlScript> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required);
    }
}
