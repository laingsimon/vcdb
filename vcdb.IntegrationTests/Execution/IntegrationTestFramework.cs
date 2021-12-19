using NUnit.Framework;
using System;
using System.Threading;
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

        public async Task Execute(IntegrationTestOptions options, CancellationToken cancellationToken = default)
        {
            if (!docker.IsInstalled())
            {
                throw new InvalidOperationException("Docker is not installed");
            }

            var startResult = await docker.IsDockerHostRunning(cancellationToken);
            switch (startResult)
            {
                case StartResult.NotStarted:
                    if (!await docker.StartDockerHost(cancellationToken))
                    {
                        Assert.Fail("Unable to start docker host");
                    }
                    break;
                case StartResult.Unstartable:
                    Assert.Fail("Unable to start docker host");
                    return;
            }

            if (!await docker.IsContainerRunning(cancellationToken))
            {
                await docker.StartDockerCompose(cancellationToken);
            }

            await sql.WaitForReady(attempts: 10);

            await scenarioExecutor.Execute(options.ConnectionString);
        }
    }
}
