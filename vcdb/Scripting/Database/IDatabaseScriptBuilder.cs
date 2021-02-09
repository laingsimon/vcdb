using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;

namespace vcdb.Scripting.Database
{
    public interface IDatabaseScriptBuilder
    {
        IEnumerable<IOutputable> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required);
    }
}
