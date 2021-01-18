using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting.User
{
    public interface IUserComparer
    {
        IEnumerable<UserDifference> GetUserDifferences(
            IDictionary<string, UserDetails> currentUsers,
            IDictionary<string, UserDetails> requiredUsers);
    }
}
