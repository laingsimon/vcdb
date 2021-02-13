using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.SchemaBuilding;
using vcdb.Scripting.Database;
using vcdb.Scripting.ExecutionPlan;

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
        private readonly IScriptOutputHeader outputHeader;
        private readonly IScriptExecutionPlanManager executionPlanManager;

        public Executor(
            IConnectionFactory connectionFactory,
            Options options,
            IDatabaseRepository databaseRepository,
            IOutput output,
            ILogger<Executor> logger,
            IInput input,
            IDatabaseScriptBuilder scriptBuilder,
            IScriptOutputHeader outputHeader,
            IScriptExecutionPlanManager executionPlanManager)
        {
            this.connectionFactory = connectionFactory;
            this.options = options;
            this.databaseRepository = databaseRepository;
            this.output = output;
            this.logger = logger;
            this.input = input;
            this.scriptBuilder = scriptBuilder;
            this.outputHeader = outputHeader;
            this.executionPlanManager = executionPlanManager;
        }

        public async Task Execute()
        {
            using (var connection = await connectionFactory.CreateConnection())
            {
                var database = await databaseRepository.GetDatabaseDetails(connection);
                var outputtable = await GetOutput(database);

                await outputtable.WriteToOutput(output);
            }
        }

        private async Task<IOutputable> GetOutput(DatabaseDetails database)
        {
            switch (options.Mode)
            {
                case ExecutionMode.Read:
                    return await ConstructRepresentationOutput(database);
                case ExecutionMode.Deploy:
                    return await ConstructUpgradeScriptOutput(database);
                default:
                    throw new NotSupportedException($"Mode {options.Mode} isn't supported");
            }
        }

        private async Task<IOutputable> ConstructUpgradeScriptOutput(DatabaseDetails currentDatabaseRepresentation)
        {
            var requiredDatabaseRepresentation = await input.Read<DatabaseDetails>();
            var scriptTasks = scriptBuilder.CreateUpgradeScripts(
                currentDatabaseRepresentation,
                requiredDatabaseRepresentation).ToArray();

            var executionPlan = executionPlanManager.CreateExecutionPlan(scriptTasks);

            return new OutputableCollection(
                outputHeader,
                executionPlan);
        }

        private Task<IOutputable> ConstructRepresentationOutput(DatabaseDetails database)
        {
            return Task.FromResult<IOutputable>(new OutputableObject<DatabaseDetails>(database));
        }
    }
}
