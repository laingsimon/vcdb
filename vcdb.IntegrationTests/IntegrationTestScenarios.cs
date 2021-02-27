using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using vcdb.IntegrationTests.Execution;

namespace vcdb.IntegrationTests
{
    public abstract class IntegrationTestScenarios : IEnumerable<IntegrationTestScenario>
    {
        private readonly string mode;

        public IDatabaseProduct DatabaseProduct { get; } = GetDatabaseProduct();

        protected IntegrationTestScenarios(string mode)
        {
            this.mode = mode;
        }

        private IEnumerable<IntegrationTestScenario> GetScenarioNames()
        {
            var testScenarios = Path.GetFullPath("..\\..\\..\\..\\TestScenarios");
            var filter = new ScenarioFilter(DatabaseProduct);

            foreach (var directory in Directory.GetDirectories(testScenarios))
            {
                if (filter.IsValidScenario(directory))
                {
                    var name = Path.GetFileName(directory);
                    var modeMatch = Regex.Match(name, @"(?<mode>.+?)_");

                    yield return new IntegrationTestScenario
                    {
                        Name = name,
                        Mode = modeMatch.Groups["mode"].Value
                    };
                }
            }
        }

        public IEnumerator<IntegrationTestScenario> GetEnumerator()
        {
            return GetScenarioNames().Where(s => s.Mode == mode || mode == null).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        public class Deploy : IntegrationTestScenarios
        {
            public Deploy()
                :base("Deploy")
            { }
        }

        public class Read : IntegrationTestScenarios
        {
            public Read()
                : base("Read")
            { }
        }
    }
}
