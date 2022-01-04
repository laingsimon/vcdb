using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.MySql.SchemaBuilding.Internal;
using vcdb.MySql.SchemaBuilding.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlCheckConstraintRepository : ICheckConstraintRepository
    {
        internal static readonly Version MinimumSupportedVersion = new Version(8, 0);

        private readonly Version minimumCompatibilityVersion;
        private readonly Options options;

        public MySqlCheckConstraintRepository(
            DatabaseVersion databaseVersion,
            Options options)
        {
            this.minimumCompatibilityVersion = databaseVersion.MinimumCompatibilityVersion == null
                ? new Version(0, 0)
                : new Version(databaseVersion.MinimumCompatibilityVersion.Contains(".") 
                    ? databaseVersion.MinimumCompatibilityVersion
                    : databaseVersion.MinimumCompatibilityVersion + ".0");
            this.options = options;
        }

        public async Task<IEnumerable<CheckConstraintDetails>> GetCheckConstraints(DbConnection connection, ObjectName tableName)
        {
            if (this.minimumCompatibilityVersion < MinimumSupportedVersion)
            {
                //the information_schema.check_constraints table only exists from versiob 8 of mysql
                return new CheckConstraintDetails[0];
            }

            var checks = await connection.QueryAsync<CheckConstraint>(
                @"select tc.constraint_name, tc.constraint_type, tc.enforced, cc.check_clause
from information_schema.table_constraints tc
left join information_schema.check_constraints cc
on cc.constraint_schema = tc.table_schema
and cc.constraint_name = tc.constraint_name
and cc.constraint_catalog = tc.constraint_catalog
where tc.table_schema = @databaseName
and tc.table_name = @table_name
and tc.constraint_type = 'CHECK';", 
                new { table_name = tableName.Name, databaseName = options.Database });

            return checks
                .Select(chk => new CheckConstraintDetails 
                {
                    Check = chk.check_clause.UnwrapDefinition(),
                    ColumnNames = GetColumnNames(chk).ToArray(),
                    Name = IsSystemNamed(tableName, chk.constraint_name)
                        ? null
                        : chk.constraint_name,
                    SqlName = chk.constraint_name,
                    CheckObjectId = null
                })
                .ToArray();
        }

        private IEnumerable<string> GetColumnNames(CheckConstraint chk)
        {
            if (string.IsNullOrEmpty(chk.check_clause))
                return new string[0];

            var matches = Regex.Matches(chk.check_clause, @"`(.+?)`");
            return matches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value);
        }

        private static bool IsSystemNamed(ObjectName tableName, string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            //Person_chk_1 is the format of a system named check
            return Regex.IsMatch(name, $@"^{tableName.Name}_chk_\d+$");
        }
    }
}
