using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.User
{
    public interface IUserScriptBuilder
    {
        IEnumerable<IOutputable> CreateUpgradeScripts(IReadOnlyCollection<UserDifference> userDifferences);
    }
}
