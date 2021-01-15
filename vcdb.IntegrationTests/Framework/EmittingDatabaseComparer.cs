using Newtonsoft.Json;
using System.IO;
using vcdb.Models;
using vcdb.Scripting;

namespace vcdb.IntegrationTests.Framework
{
    public class EmittingDatabaseComparer : IDatabaseComparer
    {
        private readonly IDatabaseComparer databaseComparer;
        private readonly DirectoryInfo scenario;

        public EmittingDatabaseComparer(DatabaseComparer databaseComparer, DirectoryInfo scenario)
        {
            this.databaseComparer = databaseComparer;
            this.scenario = scenario;
        }

        public DatabaseDifference GetDatabaseDifferences(ComparerContext context, DatabaseDetails currentDatabase, DatabaseDetails requiredDatabase)
        {
            var difference = databaseComparer.GetDatabaseDifferences(context, currentDatabase, requiredDatabase);

            var outputFilePath = Path.Combine(scenario.FullName, "Differences.json");
            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(difference, Formatting.Indented));

            return difference;
        }
    }
}
