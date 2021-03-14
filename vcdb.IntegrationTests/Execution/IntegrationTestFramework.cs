using System;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestFramework
    {
        private readonly ISql sql;
        private readonly IDocker docker;
        private readonly ScenarioExecutor scenarioExecutor;

        public IntegrationTestFramework(
            ISql sql,
            IDocker docker,
            ScenarioExecutor scenarioExecutor)
        {
            this.sql = sql;
            this.docker = docker;
            this.scenarioExecutor = scenarioExecutor;
        }

        public async Task Execute(IntegrationTestOptions options)
        {
            if (!docker.IsInstalled())
            {
                throw new InvalidOperationException("Docker is not installed");
            }

            if (!await docker.IsDockerHostRunning())
            {
                await docker.StartDockerHost();
            }

            if (!await docker.IsContainerRunning())
            {
                await docker.StartDockerCompose();
            }

            await sql.WaitForReady(attempts: 10);

            await scenarioExecutor.Execute(options.ConnectionString);
        }
    }
}
