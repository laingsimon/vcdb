using System.Collections.Generic;
using vcdb.Scripting.Schema;
using vcdb.Scripting.Table;

namespace vcdb.Scripting.Database
{
    public class DatabaseDifference
    {
        public IReadOnlyCollection<TableDifference> TableDifferences { get; set; }
        public IReadOnlyCollection<SchemaDifference> SchemaDifferences { get; set; }

        public Change<string> DescriptionChangedTo { get; set; }
        public string CollationChangedTo { get; set; }
    }
}
