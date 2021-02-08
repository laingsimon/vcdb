using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.SchemaBuilding
{
    public interface IDescriptionRepository
    {
        Task<IDictionary<string, string>> GetColumnDescriptions(DbConnection connection, ObjectName tableName);
        Task<string> GetDatabaseDescription(DbConnection connection);
        Task<IDictionary<string, string>> GetIndexDescriptions(DbConnection connection, ObjectName tableName);
        Task<string> GetSchemaDescription(DbConnection connection, string schemaName);
        Task<string> GetTableDescription(DbConnection connection, ObjectName tableName);
        Task<string> GetPrimaryKeyDescription(DbConnection connection, ObjectName tableName, string primaryKeyName);
        Task<string> GetProcedureDescription(DbConnection connection, ObjectName procedureName);
        Task<IDictionary<string, string>> GetForeignKeyDescription(DbConnection connection, ObjectName tableName);
    }
}