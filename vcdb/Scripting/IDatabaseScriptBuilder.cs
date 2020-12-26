using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface IDatabaseScriptBuilder
    {
        IAsyncEnumerable<string> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required);
    }
}
