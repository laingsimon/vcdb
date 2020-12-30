using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface ITableScriptBuilder
    {
        IAsyncEnumerable<string> CreateUpgradeScripts(
            IDictionary<TableName, TableDetails> current,
            IDictionary<TableName, TableDetails> required);
    }
}
