using System;
using System.Text.RegularExpressions;

namespace vcdb.IntegrationTests
{
    public class ProductNameScenario
    {
        public ProductNameScenario(ProductName productName, string scenarioName)
        {
            ProductName = productName;
            ScenarioName = scenarioName;
        }

        public ProductName ProductName { get; }
        public string ScenarioName { get; }

        public override string ToString()
        {
            return $"{ProductName}, {ScenarioName}";
        }
    }
}
