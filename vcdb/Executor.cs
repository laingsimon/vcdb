using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace vcdb
{
    public class Executor : IExecutor
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly Options options;
        private readonly IDatabaseRepository databaseRepository;
        private readonly IOutput output;
        private readonly ILogger<Executor> logger;

        public Executor(
            IConnectionFactory connectionFactory,
            Options options,
            IDatabaseRepository databaseRepository,
            IOutput output,
            ILogger<Executor> logger)
        {
            this.connectionFactory = connectionFactory;
            this.options = options;
            this.databaseRepository = databaseRepository;
            this.output = output;
            this.logger = logger;
        }

        public async Task Execute()
        {
            using (var connection = await connectionFactory.CreateConnection())
            {
                try
                {
                    logger.LogInformation($"Reading database objects...");
                    var database = await databaseRepository.GetDatabaseDetails(connection);
                    var outputtable = await GetOutput(database);

                    await outputtable.WriteToOutput(output);
                }
                finally
                {
                    logger.LogInformation($"Finished");
                }
            }
        }

        private async Task<IOutputable> GetOutput(DatabaseDetails database)
        {
            switch (options.Mode)
            {
                case ExecutionMode.Construct:
                    return await ConstructRepresentationOutput(database);
                case ExecutionMode.Differential:
                    return await ConstructUpgradeScriptOutput(database);
                default:
                    throw new NotSupportedException($"Mode {options.Mode} isn't supported");
            }
        }

        private Task<IOutputable> ConstructUpgradeScriptOutput(DatabaseDetails database)
        {
            //construct a SQL script that is required to change the database from <database> to the <input representation>
            throw new NotImplementedException();
        }

        private Task<IOutputable> ConstructRepresentationOutput(DatabaseDetails database)
        {
            return Task.FromResult<IOutputable>(new OutputableObject<DatabaseDetails>(database));
        }
    }
}