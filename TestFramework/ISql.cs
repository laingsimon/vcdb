using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace TestFramework
{
    internal interface ISql
    {
        Task WaitForReady(int attempts);
        Task ExecuteBatchedSql(TextReader sqlContent);
        Task ExecuteSql(string sql);
        Task ExecuteSql(string sql, SqlConnection connection);
    }
}