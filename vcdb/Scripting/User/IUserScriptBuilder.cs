using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.User
{
    public interface IUserScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<UserDifference> userDifferences);
    }
}
