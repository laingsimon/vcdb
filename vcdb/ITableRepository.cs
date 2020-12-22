using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb
{
    public interface ITableRepository
    {
        Task<Dictionary<string, TableDetails>> GetTables(DbConnection connection);
    }
}