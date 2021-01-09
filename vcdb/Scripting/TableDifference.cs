using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class TableDifference
    {
        public NamedItem<TableName, TableDetails> CurrentTable { get; set; }
        public NamedItem<TableName, TableDetails> RequiredTable { get; set; }

        public TableName TableRenamedTo { get; set; }
        public bool TableAdded { get; set; }
        public bool TableDeleted { get; set; }
        public Change<string> DescriptionChangedTo { get; set; }

        public IReadOnlyCollection<ColumnDifference> ColumnDifferences { get; set; }
        public IReadOnlyCollection<IndexDifference> IndexDifferences { get; set; }
        public IReadOnlyCollection<CheckConstraintDifference> ChangedCheckConstraints { get; set; }

        public bool IsChanged
        {
            get
            {
                return TableAdded
                    || TableDeleted
                    || TableRenamedTo != null
                    || ColumnDifferences?.Any() == true
                    || IndexDifferences?.Any() == true
                    || DescriptionChangedTo != null
                    || ChangedCheckConstraints?.Any() == true;
            }
        }
    }
}
