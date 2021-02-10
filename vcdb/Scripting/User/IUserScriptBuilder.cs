using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.User
{
    public interface IUserScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(IReadOnlyCollection<UserDifference> userDifferences);
    }
}
