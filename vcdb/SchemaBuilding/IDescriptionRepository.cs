using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.SchemaBuilding
{
    public interface IDescriptionRepository
    {
        Task<Dictionary<string, string>> GetColumnDescriptions(DbConnection connection, ObjectName tableName);
        Task<string> GetDatabaseDescription(DbConnection connection);
        Task<Dictionary<string, string>> GetIndexDescriptions(DbConnection connection, ObjectName tableName);
        Task<string> GetSchemaDescription(DbConnection connection, string schemaName);
        Task<string> GetTableDescription(DbConnection connection, ObjectName tableName);
        Task<string> GetPrimaryKeyDescription(DbConnection connection, ObjectName tableName, string primaryKeyName);
        Task<string> GetProcedureDescription(DbConnection connection, ObjectName procedureName);
        Task<Dictionary<string, string>> GetForeignKeyDescription(DbConnection connection, ObjectName tableName);
    }
}