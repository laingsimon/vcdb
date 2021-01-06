using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class IndexDifference
    {
        public const string UnchangedDescription = "\0";

        public NamedItem<string, IndexDetails> CurrentIndex { get; set; }
        public NamedItem<string, IndexDetails> RequiredIndex { get; set; }
        public bool IndexDeleted { get; set; }
        public bool IndexAdded { get; set; }
        public string IndexRenamedTo { get; set; }
        public bool? ClusteredChangedTo { get; set; }
        public bool? UniqueChangedTo { get; set; }
        public IReadOnlyCollection<IndexColumnDetailsDifference> ChangedIncludedColumns { get; set; }
        public IReadOnlyCollection<IndexColumnDetailsDifference> ChangedColumns { get; set; }
        public string DescriptionChangedTo { get; set; } = UnchangedDescription;

        public bool IsChanged
        {
            get
            {
                return IndexAdded
                    || IndexDeleted
                    || IndexRenamedTo != null
                    || ClusteredChangedTo != null
                    || UniqueChangedTo != null
                    || ChangedColumns?.Any() == true
                    || ChangedIncludedColumns?.Any() == true
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
