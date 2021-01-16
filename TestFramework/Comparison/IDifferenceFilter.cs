using System.Collections.Generic;

namespace TestFramework.Comparison
{
    public interface IDifferenceFilter
    {
        IEnumerable<Difference> FilterDifferences(IEnumerable<Difference> differences);
    }
}