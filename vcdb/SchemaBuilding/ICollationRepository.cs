using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.SchemaBuilding
{
    public interface ICollationRepository
    {
        Task<IDictionary<string, string>> GetColumnCollations(DbConnection connection, ObjectName tableName);
        Task<string> GetDatabaseCollation(DbConnection connection);
        Task<string> GetServerCollation(DbConnection connection);
    }
}
