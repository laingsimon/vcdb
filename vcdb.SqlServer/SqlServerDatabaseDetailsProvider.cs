using System.Data.SqlClient;
using vcdb.CommandLine;

namespace vcdb.SqlServer
{
    public class SqlServerDatabaseDetailsProvider : IDatabaseDetailsProvider
    {
        private readonly Options options;

        public SqlServerDatabaseDetailsProvider(Options options)
        {
            this.options = options;
        }

        public string GetServerName()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(options.ConnectionString);
            return connectionStringBuilder.DataSource;
        }
    }
}
