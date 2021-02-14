using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    internal interface ISql
    {
        Task WaitForReady(int attempts);
        Task ExecuteBatchedSql(TextReader sqlContent, string database = null);
        Task ExecuteSql(string sql, string database = null);
        Task ExecuteSql(string sql, DbConnection connection);
    }
}