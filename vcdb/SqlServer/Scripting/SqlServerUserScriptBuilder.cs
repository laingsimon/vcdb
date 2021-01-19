using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting.User;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerUserScriptBuilder : IUserScriptBuilder
    {
        public IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<UserDifference> userDifferences)
        {
            foreach (var userDifference in userDifferences)
            {
                var requiredUser = userDifference.RequiredUser;
                var currentUser = userDifference.CurrentUser;

                if (userDifference.UserAdded)
                {
                    foreach (var script in GetCreateUserScript(requiredUser))
                    {
                        yield return script;
                    }

                    continue;
                }

                if (userDifference.UserDeleted)
                {
                    yield return GetDropUserScript(currentUser.Key);
                    continue;
                }

                if (userDifference.LoginChangedTo != null)
                {
                    foreach (var script in DropAndRecreateUser(currentUser.Key, requiredUser))
                        yield return script;

                    continue;
                }

                if (userDifference.UserRenamedTo != null || userDifference.DefaultSchemaChangedTo != null)
                {
                    foreach (var script in GetAlterUserScript(currentUser.Key, userDifference))
                        yield return script;
                }

                if (userDifference.StateChangedTo != null)
                {
                    yield return GetChangeLoginStateScript(requiredUser.Value.LoginName, userDifference.StateChangedTo.Value);
                }
            }
        }

        private IEnumerable<SqlScript> GetAlterUserScript(string currentName, UserDifference userDifference)
        {
            var requiredUser = userDifference.RequiredUser;

            var withClauses = new[] 
            {
                userDifference.UserRenamedTo != null
                    ? $"NAME = {requiredUser.Key.SqlSafeName()}"
                    : "",
                userDifference.DefaultSchemaChangedTo != null
                    ? $"DEFAULT_SCHEMA = {userDifference.DefaultSchemaChangedTo.SqlSafeName()}"
                    : ""
            }.Where(clause => !string.IsNullOrEmpty(clause)).ToArray();
            var clauses = string.Join("," + Environment.NewLine, withClauses);

            yield return new SqlScript($@"ALTER USER {currentName.SqlSafeName()}
WITH {clauses}
GO");
        }

        private IEnumerable<SqlScript> DropAndRecreateUser(string userName, NamedItem<string, UserDetails> requiredUser)
        {
            yield return GetDropUserScript(userName);
            foreach (var script in GetCreateUserScript(requiredUser))
            {
                yield return script;
            }
        }

        private SqlScript GetDropUserScript(string userName)
        {
            return new SqlScript($@"DROP USER {userName.SqlSafeName()}
GO");
        }

        private IEnumerable<SqlScript> GetCreateUserScript(NamedItem<string, UserDetails> requiredUser)
        {
            yield return new SqlScript($@"CREATE USER {requiredUser.Key.SqlSafeName()} FOR LOGIN {requiredUser.Value.LoginName.SqlSafeName()}
GO");

            if (!requiredUser.Value.Enabled)
            {
                yield return GetChangeLoginStateScript(requiredUser.Value.LoginName, requiredUser.Value.Enabled);
            }

            if (requiredUser.Value.DefaultSchema != null)
            {
                yield return GetChangeDefaultSchemaScript(requiredUser);
            }
        }

        private SqlScript GetChangeDefaultSchemaScript(NamedItem<string, UserDetails> requiredUser)
        {
            return new SqlScript($@"ALTER USER {requiredUser.Key.SqlSafeName()}
WITH DEFAULT_SCHEMA = {requiredUser.Value.DefaultSchema.SqlSafeName()}
GO");
        }

        private SqlScript GetChangeLoginStateScript(string loginName, bool enabled)
        {
            var disabledClause = enabled
                ? "ENABLE"
                : "DISABLE";

            return new SqlScript($@"ALTER LOGIN {loginName.SqlSafeName()}
{disabledClause}
GO");
        }
    }
}
