using System.Collections.Generic;

namespace vcdb.Scripting
{
    public class DatabaseDifference
    {
        public IReadOnlyCollection<TableDifference> TableDifferences { get; set; }
        public IReadOnlyCollection<SchemaDifference> SchemaDifferences { get; set; }
    }
}
