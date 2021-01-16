using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace TestFramework.Database
{
    public interface ISql
    {
        Task WaitForReady(int attempts);
        Task ExecuteBatchedSql(TextReader sqlContent, string database = null);
        Task ExecuteSql(string sql, string database = null);
        Task ExecuteSql(string sql, SqlConnection connection);
    }
}