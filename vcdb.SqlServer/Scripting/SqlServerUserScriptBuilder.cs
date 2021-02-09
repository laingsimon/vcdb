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
        public IEnumerable<IOutputable> CreateUpgradeScripts(IReadOnlyCollection<UserDifference> userDifferences)
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

                if (userDifference.UserRenamedTo != null || userDifference.DefaultSchemaChangedTo != null || userDifference.LoginChangedTo != null)
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

        private IEnumerable<IOutputable> GetAlterUserScript(string currentName, UserDifference userDifference)
        {
            var requiredUser = userDifference.RequiredUser;

            var withClauses = new[] 
            {
                userDifference.UserRenamedTo != null
                    ? $"NAME = {requiredUser.Key.SqlSafeName()}"
                    : "",
                userDifference.DefaultSchemaChangedTo != null
                    ? $"DEFAULT_SCHEMA = {userDifference.DefaultSchemaChangedTo.SqlSafeName()}"
                    : "",
                userDifference.LoginChangedTo != null
                    ? $"LOGIN = {userDifference.LoginChangedTo.SqlSafeName()}"
                    : ""
            }.Where(clause => !string.IsNullOrEmpty(clause)).ToArray();
            var clauses = string.Join("," + Environment.NewLine, withClauses);

            yield return new SqlScript($@"ALTER USER {currentName.SqlSafeName()}
WITH {clauses}
GO");
        }

        private SqlScript GetDropUserScript(string userName)
        {
            return new SqlScript($@"DROP USER {userName.SqlSafeName()}
GO");
        }

        private IEnumerable<IOutputable> GetCreateUserScript(NamedItem<string, UserDetails> requiredUser)
        {
            var withClauses = new[]
            {
                requiredUser.Value.DefaultSchema != null
                    ? $"DEFAULT_SCHEMA = {requiredUser.Value.DefaultSchema.SqlSafeName()}"
                    : ""
            }.Where(clause => !string.IsNullOrEmpty(clause)).ToArray();

            var clauses = withClauses.Any()
                ? @$"WITH {string.Join("," + Environment.NewLine, withClauses)}{Environment.NewLine}"
                : "";

            yield return new SqlScript($@"CREATE USER {requiredUser.Key.SqlSafeName()} FOR LOGIN {requiredUser.Value.LoginName.SqlSafeName()}
{clauses}GO");

            if (requiredUser.Value.Enabled == false)
            {
                yield return GetChangeLoginStateScript(requiredUser.Value.LoginName, requiredUser.Value.Enabled);
            }
        }

        private SqlScript GetChangeLoginStateScript(string loginName, OptOut enabled)
        {
            var disabledClause = enabled == true
                ? "ENABLE"
                : "DISABLE";

            return new SqlScript($@"ALTER LOGIN {loginName.SqlSafeName()}
{disabledClause}
GO");
        }
    }
}
