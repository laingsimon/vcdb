using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.SchemaBuilding;
using vcdb.Scripting;

namespace vcdb
{
    public class Executor : IExecutor
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly Options options;
        private readonly IDatabaseRepository databaseRepository;
        private readonly IOutput output;
        private readonly ILogger<Executor> logger;
        private readonly IInput input;
        private readonly IDatabaseScriptBuilder scriptBuilder;

        public Executor(
            IConnectionFactory connectionFactory,
            Options options,
            IDatabaseRepository databaseRepository,
            IOutput output,
            ILogger<Executor> logger,
            IInput input,
            IDatabaseScriptBuilder scriptBuilder)
        {
            this.connectionFactory = connectionFactory;
            this.options = options;
            this.databaseRepository = databaseRepository;
            this.output = output;
            this.logger = logger;
            this.input = input;
            this.scriptBuilder = scriptBuilder;
        }

        public async Task Execute()
        {
            using (var connection = await connectionFactory.CreateConnection())
            {
                logger.LogInformation($"Reading database objects...");
                var database = await databaseRepository.GetDatabaseDetails(connection);
                var outputtable = await GetOutput(database);

                logger.LogInformation($"Writing output...");
                await outputtable.WriteToOutput(output);
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

        private async Task<IOutputable> ConstructUpgradeScriptOutput(DatabaseDetails currentDatabaseRepresentation)
        {
            var requiredDatabaseRepresentation = await input.Read<DatabaseDetails>();
            var databaseScripts = scriptBuilder.CreateUpgradeScripts(
                currentDatabaseRepresentation,
                requiredDatabaseRepresentation);
            return new EnumerableOutput<SqlScript>(databaseScripts);
        }

        private Task<IOutputable> ConstructRepresentationOutput(DatabaseDetails database)
        {
            return Task.FromResult<IOutputable>(new OutputableObject<DatabaseDetails>(database));
        }
    }
}