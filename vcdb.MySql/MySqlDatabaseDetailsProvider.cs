using MySql.Data.MySqlClient;
using vcdb.CommandLine;

namespace vcdb.MySql
{
    public class MySqlDatabaseDetailsProvider : IDatabaseDetailsProvider
    {
        private readonly Options options;

        public MySqlDatabaseDetailsProvider(Options options)
        {
            this.options = options;
        }

        public string GetServerName()
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder(options.ConnectionString);
            return connectionStringBuilder.Server;
        }
    }
}
