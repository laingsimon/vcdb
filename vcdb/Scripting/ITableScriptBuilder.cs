using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface ITableScriptBuilder
    {
        IAsyncEnumerable<string> CreateUpgradeScripts(
            IDictionary<string, TableDetails> current,
            IDictionary<string, TableDetails> required);
    }
}
