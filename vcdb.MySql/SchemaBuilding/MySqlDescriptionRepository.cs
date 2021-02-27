using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlDescriptionRepository : IDescriptionRepository
    {
        public Task<Dictionary<string, string>> GetColumnDescriptions(DbConnection connection, ObjectName tableName)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }

        public Task<string> GetDatabaseDescription(DbConnection connection)
        {
            return Task.FromResult<string>(null);
        }

        public Task<Dictionary<string, string>> GetForeignKeyDescription(DbConnection connection, ObjectName tableName)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }

        public Task<Dictionary<string, string>> GetIndexDescriptions(DbConnection connection, ObjectName tableName)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }

        public Task<string> GetPrimaryKeyDescription(DbConnection connection, ObjectName tableName, string primaryKeyName)
        {
            return Task.FromResult<string>(null);
        }

        public Task<string> GetProcedureDescription(DbConnection connection, ObjectName procedureName)
        {
            return Task.FromResult<string>(null);
        }

        public Task<string> GetSchemaDescription(DbConnection connection, string schemaName)
        {
            return Task.FromResult<string>(null);
        }

        public Task<string> GetTableDescription(DbConnection connection, ObjectName tableName)
        {
            return Task.FromResult<string>(null);
        }
    }
}
