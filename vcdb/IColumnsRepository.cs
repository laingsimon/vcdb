using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb
{
    public interface IColumnsRepository
    {
        Task<Dictionary<string, ColumnDetails>> GetColumns(DbConnection connection, TableIdentifier tableIdentifier);
    }
}