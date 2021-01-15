using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;

namespace vcdb.Scripting.Database
{
    public interface IDatabaseScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required);
    }
}
