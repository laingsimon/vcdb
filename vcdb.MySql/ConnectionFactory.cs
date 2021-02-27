using Dapper;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.CommandLine;

namespace vcdb.MySql
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly Options options;
        private readonly ILogger<ConnectionFactory> logger;

        public ConnectionFactory(Options options, ILogger<ConnectionFactory> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public async Task<DbConnection> CreateConnection()
        {
            var connection = new MySqlConnection(options.ConnectionString);

            logger.LogDebug($"Opening connection...");

            await connection.OpenAsync();
            if (!string.IsNullOrEmpty(options.Database))
            {
                logger.LogDebug($"Switching to database {options.Database}...");
                await connection.ExecuteAsync($"use `{options.Database}`");
            }

            return connection;
        }
    }
}
