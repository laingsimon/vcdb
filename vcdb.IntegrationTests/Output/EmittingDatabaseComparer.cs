using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using vcdb.Models;
using vcdb.Scripting;
using vcdb.Scripting.Database;

namespace vcdb.IntegrationTests.Output
{
    internal class EmittingDatabaseComparer : IDatabaseComparer
    {
        private readonly IDatabaseComparer databaseComparer;
        private readonly DirectoryInfo scenario;
        private readonly IDatabaseProduct databaseProduct;

        public EmittingDatabaseComparer(DatabaseComparer databaseComparer, DirectoryInfo scenario, IDatabaseProduct databaseProduct)
        {
            this.databaseComparer = databaseComparer;
            this.scenario = scenario;
            this.databaseProduct = databaseProduct;
        }

        public DatabaseDifference GetDatabaseDifferences(ComparerContext context, DatabaseDetails currentDatabase, DatabaseDetails requiredDatabase)
        {
            var difference = databaseComparer.GetDatabaseDifferences(context, currentDatabase, requiredDatabase);

            var outputFilePath = Path.Combine(scenario.FullName, $"Differences.{databaseProduct.Name}.json");
            File.WriteAllText(
                outputFilePath,
                JsonConvert.SerializeObject(
                    difference,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        Converters =
                        {
                            new StringEnumConverter()
                        }
                    }));

            return difference;
        }
    }
}
