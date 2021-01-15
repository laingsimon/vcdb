using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.SchemaBuilding
{
    public interface IDescriptionRepository
    {
        Task<IDictionary<string, string>> GetColumnDescriptions(DbConnection connection, TableName tableName);
        Task<string> GetDatabaseDescription(DbConnection connection);
        Task<IDictionary<string, string>> GetIndexDescriptions(DbConnection connection, TableName tableName);
        Task<string> GetSchemaDescription(DbConnection connection, string schemaName);
        Task<string> GetTableDescription(DbConnection connection, TableName tableName);
        Task<string> GetPrimaryKeyDescription(DbConnection connection, TableName tableName, string primaryKeyName);
    }
}