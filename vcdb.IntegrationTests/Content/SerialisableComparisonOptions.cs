using JsonEqualityComparer;

namespace vcdb.IntegrationTests.Content
{
    public class SerialisableComparisonOptions : ComparisonOptions
    {
        public bool IgnoreLineEndings { get; set; }

        public bool IgnoreCase { get; set; }
    }
}
