using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerUserRepository : IUserRepository
    {
        public static readonly HashSet<string> LoginsToIgnore = new HashSet<string>
        {
            "##MS_AgentSigningCertificate##",
            "##MS_PolicyEventProcessingLogin##",
            "##MS_PolicySigningCertificate##",
            "##MS_PolicyTsqlExecutionLogin##",
            "##MS_SmoExtendedSigningCertificate##",
            "##MS_SQLAuthenticatorCertificate##",
            "##MS_SQLReplicationSigningCertificate##",
            "##MS_SQLResourceSigningCertificate##",
            "NT AUTHORITY\\NETWORK SERVICE",
            "NT AUTHORITY\\SYSTEM",
            "sa"
        };
        private readonly Options options;

        public SqlServerUserRepository(Options options)
        {
            this.options = options;
        }

        public async Task<Dictionary<string, UserDetails>> GetUsers(DbConnection connection)
        {
            var users = await connection.QueryAsync<UserAndLogin>(@"
select db.name,
       db.type_desc,
       db.authentication_type_desc,
       db.default_schema_name,
       svr.name as login,
       svr.is_disabled
from sys.database_principals db
inner join sys.server_principals svr
on svr.sid = db.sid
where db.type not in ('A', 'G', 'R', 'X')
      and db.sid is not null
      and db.name != 'guest'");

            return users
                .Where(user => !LoginsToIgnore.Contains(user.login))
                .ToDictionary(
                    user => user.name,
                    user => new UserDetails
                    {
                        Type = GetLoginType(user.type_desc),
                        Enabled = !user.is_disabled,
                        LoginName = user.login,
                        DefaultSchema = user.default_schema_name == options.UserDefaultSchemaName
                            ? null
                            : user.default_schema_name
                    });
        }

        private UserType GetLoginType(string type_desc)
        {
            switch (type_desc)
            {
                case "SQL_USER":
                    return UserType.DatabaseAuthority;
                case "WINDOWS_USER":
                case "EXTERNAL_USER":
                    return UserType.WindowsAuthority;
                case "CERTIFICATE_MAPPED_USER":
                    return UserType.CertificateAuthority;
                case "ASYMMETRIC_KEY_MAPPED_USER":
                    return UserType.KeyAuthority;
            }

            throw new NotSupportedException($"Login type '{type_desc}' isn't supported");
        }
    }
}
