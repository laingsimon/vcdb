using System.IO;

namespace vcdb.IntegrationTests.Execution
{
    internal class ScenarioFilter
    {
        private readonly ProductName productName;

        public ScenarioFilter(ProductName productName)
        {
            this.productName = productName;
        }

        public bool IsValidScenario(DirectoryInfo directory)
        {
            return IsValidScenario(directory.FullName);
        }

        public bool IsValidScenario(string directory)
        {
            return File.Exists(Path.Combine(directory, $"ExpectedOutput.{productName}.sql"))
                || File.Exists(Path.Combine(directory, $"Database.{productName}.sql"));
        }
    }
}
