using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting.ForeignKey
{
    public class ForeignKeyDifference
    {
        public NamedItem<string, ForeignKeyDetails> RequiredForeignKey { get; set; }
        public NamedItem<string, ForeignKeyDetails> CurrentForeignKey { get; set; }
        public bool ForeignKeyAdded { get; set; }
        public bool ForeignKeyDeleted { get; set; }
        public Change<string> DescriptionChangedTo { get; set; }
        public string ForeignKeyRenamedTo { get; set; }
        public IReadOnlyCollection<ForeignKeyColumnDifference> ChangedColumns { get; set; }

        public bool IsChanged
        {
            get
            {
                return ForeignKeyAdded
                    || ForeignKeyDeleted
                    || DescriptionChangedTo != null
                    || ForeignKeyRenamedTo != null
                    || ChangedColumns?.Any() == true;
            }
        }
    }
}
