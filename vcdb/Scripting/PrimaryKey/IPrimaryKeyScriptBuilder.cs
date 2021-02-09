using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.PrimaryKey
{
    public interface IPrimaryKeyScriptBuilder
    {
        IEnumerable<IOutputable> CreateUpgradeScripts(
            ObjectName tableName,
            PrimaryKeyDifference primaryKeyDifference,
            ScriptingPhase phase);
    }
}
