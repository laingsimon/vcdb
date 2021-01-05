using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;

namespace vcdb.Scripting
{
    public interface ITableScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(
            IDictionary<TableName, TableDetails> current,
            IDictionary<TableName, TableDetails> required);
    }
}
