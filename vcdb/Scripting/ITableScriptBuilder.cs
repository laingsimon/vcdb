using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;

namespace vcdb.Scripting
{
    public interface ITableScriptBuilder
    {
        IAsyncEnumerable<SqlScript> CreateUpgradeScripts(
            IDictionary<TableName, TableDetails> current,
            IDictionary<TableName, TableDetails> required);
    }
}
