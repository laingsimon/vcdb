using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.User;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerUserScriptBuilder : IUserScriptBuilder
    {
        public IEnumerable<IScriptTask> CreateUpgradeScripts(IReadOnlyCollection<UserDifference> userDifferences)
        {
            foreach (var userDifference in userDifferences)
            {
                var requiredUser = userDifference.RequiredUser;
                var currentUser = userDifference.CurrentUser;

                if (userDifference.UserAdded)
                {
                    yield return new MultiScriptTask(GetCreateUserScript(requiredUser));

                    continue;
                }

                if (userDifference.UserDeleted)
                {
                    yield return GetDropUserScript(currentUser.Key);
                    continue;
                }

                if (userDifference.UserRenamedTo != null || userDifference.DefaultSchemaChangedTo != null || userDifference.LoginChangedTo != null)
                {
                    yield return GetAlterUserScript(currentUser.Key, userDifference);
                }

                if (userDifference.StateChangedTo != null)
                {
                    yield return GetChangeLoginStateScript(requiredUser.Value.LoginName, userDifference.StateChangedTo.Value);
                }
            }
        }

        private IScriptTask GetAlterUserScript(string currentName, UserDifference userDifference)
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

            return new SqlScript($@"ALTER USER {currentName.SqlSafeName()}
WITH {clauses}
GO").CreatesOrAlters().User(requiredUser.Key);
        }

        private IScriptTask GetDropUserScript(string userName)
        {
            return new SqlScript($@"DROP USER {userName.SqlSafeName()}
GO").Drops().User(userName);
        }

        private IEnumerable<IScriptTask> GetCreateUserScript(NamedItem<string, UserDetails> requiredUser)
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
{clauses}GO").CreatesOrAlters().User(requiredUser.Key);

            if (requiredUser.Value.Enabled == false)
            {
                yield return GetChangeLoginStateScript(requiredUser.Value.LoginName, requiredUser.Value.Enabled);
            }
        }

        private IScriptTask GetChangeLoginStateScript(string loginName, OptOut enabled)
        {
            var disabledClause = enabled == true
                ? "ENABLE"
                : "DISABLE";

            return new SqlScript($@"ALTER LOGIN {loginName.SqlSafeName()}
{disabledClause}
GO").AsTask();
        }
    }
}
