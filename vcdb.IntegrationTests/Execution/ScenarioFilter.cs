using System.IO;

namespace vcdb.IntegrationTests.Execution
{
    internal class ScenarioFilter
    {
        private readonly IDatabaseProduct databaseProduct;

        public ScenarioFilter(IDatabaseProduct databaseProduct)
        {
            this.databaseProduct = databaseProduct;
        }

        public bool IsValidScenario(DirectoryInfo directory)
        {
            return IsValidScenario(directory.FullName);
        }

        public bool IsValidScenario(string directory)
        {
            return File.Exists(Path.Combine(directory, $"ExpectedOutput.{databaseProduct.Name}.sql"))
                || File.Exists(Path.Combine(directory, $"Database.{databaseProduct.Name}.sql"));
        }
    }
}
