using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using vcdb.CommandLine;

namespace vcdb.IntegrationTests
{
    public abstract class IntegrationTestScenarios : IEnumerable<IntegrationTestScenario>
    {
        public static IDatabaseProduct DatabaseProduct { get; set; }

        private readonly ExecutionMode mode;
        private readonly IDatabaseProduct databaseProduct;

        protected IntegrationTestScenarios(ExecutionMode mode, IDatabaseProduct databaseProduct = null)
        {
            databaseProduct ??= GetDatabaseProduct();

            this.mode = mode;
            this.databaseProduct = databaseProduct;
        }

        internal static IDatabaseProduct GetDatabaseProduct()
        {
            var databaseProductInterface = typeof(IDatabaseProduct);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var relevantAssemblies = assemblies
                .Except(new[] { databaseProductInterface.Assembly })
                .Where(assembly => assembly.GetName().Name.StartsWith("vcdb.", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var databaseProducts = relevantAssemblies.SelectMany(assembly => assembly.GetTypes().Where(type => databaseProductInterface.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)).ToArray();
            var scannedAssemblies = string.Join("\r\n", relevantAssemblies.Select(a => $" - {a.GetName().Name}"));

            if (!databaseProducts.Any())
            {
                throw new InvalidOperationException(@$"A class must be created that implements {databaseProductInterface.FullName}
Scanned assemblies:
{scannedAssemblies}");
            }

            if (databaseProducts.Length == 1)
            {
                return CreateDatabaseProduct(databaseProducts.Single());
            }

            throw new InvalidOperationException(@$"Multiple ({databaseProducts.Length}) implementors of {databaseProductInterface.FullName} found, unable to resolve imbiguity
Found types:
{string.Join("\r\n", databaseProducts.Select(type => $" - {type.FullName}"))}
Scanned assemblies:
{scannedAssemblies}");
        }

        private static IDatabaseProduct CreateDatabaseProduct(Type type)
        {
            return (IDatabaseProduct)Activator.CreateInstance(type);
        }

        private IEnumerable<IntegrationTestScenario> GetScenarioNames()
        {
            var testScenarios = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "TestScenarios"));
            var repository = new IntegrationTestDirectoryRepository(new DirectoryInfo(testScenarios));
            var fileGroups = repository.GetDirectories()
                .Where(d => d.Mode == mode)
                .SelectMany(d => d.GetFileGroups())
                .Where(fg => fg.DatabaseVersion.ProductName == databaseProduct.Name);

            foreach (var fileGroup in fileGroups)
            {
                if (!IsPermitted(fileGroup.DatabaseVersion))
                {
                    continue;
                }

                yield return new IntegrationTestScenario(databaseProduct, fileGroup);
            }
        }

        private bool IsPermitted(DatabaseVersion databaseVersion)
        {
            var blacklistedDatabaseVersions = Environment.GetEnvironmentVariable("Vcdb_Blacklisted_DatabaseVersions") ?? "";
            var items = blacklistedDatabaseVersions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var blacklisted = items.Any(item => item == databaseVersion.ProductName || item == databaseVersion.ToString());
            return !blacklisted;
        }

        public IEnumerator<IntegrationTestScenario> GetEnumerator()
        {
            return GetScenarioNames().Where(s => s.FileGroup.Mode == mode).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class Deploy : IntegrationTestScenarios
        {
            public Deploy()
                :base(ExecutionMode.Deploy)
            { }
        }

        public class Read : IntegrationTestScenarios
        {
            public Read()
                : base(ExecutionMode.Read)
            { }
        }
    }
}
