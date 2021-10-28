using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.MySql.SchemaBuilding.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlUserRepository : IUserRepository
    {
        private static readonly string[] ExcludedMySqlUserNames = new[]
        {
            "root@localhost",
            "mysql.session@localhost",
            "mysql.sys@localhost",
            "root@%",
        };

        private readonly Options options;

        public MySqlUserRepository(Options options)
        {
            this.options = options;
        }

        public async Task<Dictionary<string, UserDetails>> GetUsers(DbConnection connection)
        {
            var users = await connection.QueryAsync<MySqlUserDetails>(
                $@"select * from mysql.user",
                new { databaseName = options.Database });

            return users
                .Where(UserNotExcluded)
                .ToDictionary(GetUserName, CreateUserDetails);
        }

        private static bool UserNotExcluded(MySqlUserDetails user)
        {
            var currentUserName = GetUserName(user);
            return !ExcludedMySqlUserNames.Contains(currentUserName);
        }

        private static string GetUserName(MySqlUserDetails user)
        {
            return $"{user.User}@{user.Host}";
        }

        private static UserDetails CreateUserDetails(MySqlUserDetails user)
        {
            return new UserDetails
            {
                LoginName = null,
                Type = GetUserType(user),
                Enabled = GetEnabled(user),
            };
        }

        private static UserType GetUserType(MySqlUserDetails user)
        {
            switch (user.SslType)
            {
                case MySqlSslTypeEnum.X509:
                    return UserType.CertificateAuthority;
                default:
                    if (user.plugin == "mysql_native_password")
                        return UserType.DatabaseAuthority;

                    throw new NotSupportedException($"Unsure on the UserType for this user {GetUserName(user)}");
            }
        }

        private static OptOut GetEnabled(MySqlUserDetails user)
        {
            return user.account_locked == MySqlBoolEnum.Y
                ? OptOut.False
                : OptOut.True;
        }
    }
}
