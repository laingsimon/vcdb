using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerDescriptionRepository : IDescriptionRepository
    {
        public async Task<string> GetDatabaseDescription(DbConnection connection)
        {
            return await connection.QueryFirstOrDefaultAsync<string>($@"select value
from fn_listextendedproperty(default, null, null, null, null, null, null)
where name = 'MS_Description'");
        }

        public async Task<string> GetSchemaDescription(DbConnection connection, string schemaName)
        {
            return await connection.QueryFirstOrDefaultAsync<string>($@"select value
from fn_listextendedproperty(default, 'SCHEMA', '{schemaName}', null, null, null, null)
where name = 'MS_Description'");
        }

        public async Task<string> GetTableDescription(DbConnection connection, ObjectName tableName)
        {
            return await connection.QueryFirstOrDefaultAsync<string>($@"select value
from fn_listextendedproperty(default, 'SCHEMA', '{tableName.Schema}', 'TABLE', '{tableName.Name}', null, null)
where name = 'MS_Description'");
        }

        public async Task<Dictionary<string, string>> GetColumnDescriptions(DbConnection connection, ObjectName tableName)
        {
            return await GetMultipleDescription(connection, tableName, "COLUMN");
        }

        public async Task<Dictionary<string, string>> GetIndexDescriptions(DbConnection connection, ObjectName tableName)
        {
            return await GetMultipleDescription(connection, tableName, "INDEX");
        }

        public async Task<string> GetPrimaryKeyDescription(DbConnection connection, ObjectName tableName, string primaryKeyName)
        {
            return await connection.QueryFirstOrDefaultAsync<string>($@"select value
from fn_listextendedproperty(default, 'SCHEMA', '{tableName.Schema}', 'TABLE', '{tableName.Name}', 'CONSTRAINT', '{primaryKeyName}')
where name = 'MS_Description'");
        }

        public async Task<string> GetProcedureDescription(DbConnection connection, ObjectName procedureName)
        {
            return await connection.QueryFirstOrDefaultAsync<string>($@"select value
from fn_listextendedproperty(default, 'SCHEMA', '{procedureName.Schema}', 'PROCEDURE', '{procedureName.Name}', null, null)
where name = 'MS_Description'");
        }

        public async Task<Dictionary<string, string>> GetForeignKeyDescription(DbConnection connection, ObjectName tableName)
        {
            return await GetMultipleDescription(connection, tableName, "CONSTRAINT");
        }

        private async Task<Dictionary<string, string>> GetMultipleDescription(DbConnection connection, ObjectName tableName, string level2Type)
        {
            return await connection.QueryAsync<DescriptionMap>($@"select objname as [ObjectName], value as Description
from fn_listextendedproperty(default, 'SCHEMA', '{tableName.Schema}', 'TABLE', '{tableName.Name}', '{level2Type}', null)
where name = 'MS_Description'").ToDictionaryAsync(map => map.ObjectName, map => map.Description);
        }

        private class DescriptionMap
        {
            public string ObjectName { get; set; }
            public string Description { get; set; }
        }
    }
}
