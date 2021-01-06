using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class TableDifference
    {
        public const string UnchangedDescription = "\0";

        public NamedItem<TableName, TableDetails> CurrentTable { get; set; }
        public NamedItem<TableName, TableDetails> RequiredTable { get; set; }

        public TableName TableRenamedTo { get; set; }
        public bool TableAdded { get; set; }
        public bool TableDeleted { get; set; }
        public string DescriptionChangedTo { get; set; }

        public IReadOnlyCollection<ColumnDifference> ColumnDifferences { get; set; }
        public IReadOnlyCollection<IndexDifference> IndexDifferences { get; set; }

        public bool IsChanged
        {
            get
            {
                return TableAdded
                    || TableDeleted
                    || TableRenamedTo != null
                    || ColumnDifferences?.Any() == true
                    || IndexDifferences?.Any() == true
                    || DescriptionHasChanged;
            }
        }

        public bool DescriptionHasChanged
        {
            get
            {
                return DescriptionChangedTo != UnchangedDescription;
            }
        }
    }
}
