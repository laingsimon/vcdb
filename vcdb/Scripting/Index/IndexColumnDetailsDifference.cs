using vcdb.Models;

namespace vcdb.Scripting.Index
{
    public class IndexColumnDetailsDifference
    {
        public NamedItem<string, IndexColumnDetails> CurrentColumn { get; set; }
        public NamedItem<string, IndexColumnDetails> RequiredColumn { get; set; }
        public bool ColumnAdded { get; set; }
        public bool ColumnRemoved { get; set; }
        public bool? DescendingChangedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return ColumnAdded
                    || ColumnRemoved
                    || DescendingChangedTo != null;
            }
        }
    }
}
