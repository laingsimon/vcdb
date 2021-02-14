using System.Collections.Generic;

namespace vcdb.IntegrationTests.Comparison
{
    internal interface IDifferenceFilter
    {
        IEnumerable<Difference> FilterDifferences(IEnumerable<Difference> differences);
    }
}