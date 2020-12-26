using Dapper;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using vcdb.CommandLine;

namespace vcdb.Output
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
            var connection = new SqlConnection(options.ConnectionString);

            logger.LogInformation($"Opening connection...");

            await connection.OpenAsync();
            if (!string.IsNullOrEmpty(options.Database))
            {
                logger.LogInformation($"Switching to database {options.Database}...");
                await connection.ExecuteAsync($"use [{options.Database}]");
            }

            return connection;
        }
    }
}
