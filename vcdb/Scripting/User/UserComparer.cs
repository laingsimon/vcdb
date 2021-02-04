using System.Collections.Generic;
using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;

namespace vcdb.Scripting.User
{
    public class UserComparer : IUserComparer
    {
        private readonly INamedItemFinder namedItemFinder;
        private readonly Options options;

        public UserComparer(INamedItemFinder namedItemFinder, Options options)
        {
            this.namedItemFinder = namedItemFinder;
            this.options = options;
        }

        public IEnumerable<UserDifference> GetUserDifferences(
            IDictionary<string, UserDetails> currentUsers,
            IDictionary<string, UserDetails> requiredUsers)
        {
            var processedUsers = new HashSet<UserDetails>();
            foreach (var requiredUser in requiredUsers)
            {
                var currentUser = namedItemFinder.GetCurrentItem(currentUsers, requiredUser);

                if (currentUser == null)
                {
                    yield return new UserDifference
                    {
                        RequiredUser = requiredUser.AsNamedItem(),
                        UserAdded = true
                    };
                }
                else
                {
                    processedUsers.Add(currentUser.Value);

                    var difference = new UserDifference
                    {
                        CurrentUser = currentUser,
                        RequiredUser = requiredUser.AsNamedItem(),
                        UserRenamedTo = !currentUser.Key.Equals(requiredUser.Key)
                            ? requiredUser.Key
                            : null,
                        StateChangedTo = currentUser.Value.Enabled != requiredUser.Value.Enabled
                            ? (requiredUser.Value.Enabled ?? OptOut.True).AsChange()
                            : null,
                        LoginChangedTo = currentUser.Value.LoginName != requiredUser.Value.LoginName
                            ? requiredUser.Value.LoginName
                            : null,
                        DefaultSchemaChangedTo = (currentUser.Value.DefaultSchema ?? options.UserDefaultSchemaName) != (requiredUser.Value.DefaultSchema ?? options.UserDefaultSchemaName)
                            ? requiredUser.Value.DefaultSchema ?? options.UserDefaultSchemaName
                            : null
                    };

                    if (difference.IsChanged)
                    {
                        yield return difference;
                    }
                }
            }

            foreach (var currentTable in currentUsers.Where(col => !processedUsers.Contains(col.Value)))
            {
                yield return new UserDifference
                {
                    CurrentUser = currentTable.AsNamedItem(),
                    UserDeleted = true
                };
            }
        }
    }
}
