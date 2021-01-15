using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

        public async Task<string> GetTableDescription(DbConnection connection, TableName tableName)
        {
            return await connection.QueryFirstOrDefaultAsync<string>($@"select value
from fn_listextendedproperty(default, 'SCHEMA', '{tableName.Schema}', 'TABLE', '{tableName.Table}', null, null)
where name = 'MS_Description'");
        }

        public async Task<IDictionary<string, string>> GetColumnDescriptions(DbConnection connection, TableName tableName)
        {
            return await GetMultipleDescription(connection, tableName, "COLUMN");
        }

        public async Task<IDictionary<string, string>> GetIndexDescriptions(DbConnection connection, TableName tableName)
        {
            return await GetMultipleDescription(connection, tableName, "INDEX");
        }

        private async Task<IDictionary<string, string>> GetMultipleDescription(DbConnection connection, TableName tableName, string level2Type)
        {
            return (await connection.QueryAsync<DescriptionMap>($@"select objname as [ObjectName], value as Description
from fn_listextendedproperty(default, 'SCHEMA', '{tableName.Schema}', 'TABLE', '{tableName.Table}', '{level2Type}', null)
where name = 'MS_Description'")).ToDictionary(map => map.ObjectName, map => map.Description);
        }

        public async Task<string> GetPrimaryKeyDescription(DbConnection connection, TableName tableName, string primaryKeyName)
        {
            return await connection.QueryFirstOrDefaultAsync<string>($@"select value
from fn_listextendedproperty(default, 'SCHEMA', '{tableName.Schema}', 'TABLE', '{tableName.Table}', 'CONSTRAINT', '{primaryKeyName}')
where name = 'MS_Description'");
        }

        private class DescriptionMap
        {
            public string ObjectName { get; set; }
            public string Description { get; set; }
        }
    }
}
