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
        private readonly Scenario scenario;
        private readonly IDatabaseProduct databaseProduct;

        public EmittingDatabaseComparer(DatabaseComparer databaseComparer, Scenario scenario, IDatabaseProduct databaseProduct)
        {
            this.databaseComparer = databaseComparer;
            this.scenario = scenario;
            this.databaseProduct = databaseProduct;
        }

        public DatabaseDifference GetDatabaseDifferences(ComparerContext context, DatabaseDetails currentDatabase, DatabaseDetails requiredDatabase)
        {
            var difference = databaseComparer.GetDatabaseDifferences(context, currentDatabase, requiredDatabase);

            using (var outputFile = scenario.Write($"Differences.{databaseProduct.Name}.json"))
            {
                var serialised = JsonConvert.SerializeObject(
                        difference,
                        new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            Converters =
                            {
                            new StringEnumConverter()
                            }
                        });

                outputFile.WriteLine(serialised);
            }

            return difference;
        }
    }
}
