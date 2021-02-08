using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting.PrimaryKey
{
    public class PrimaryKeyDifference
    {
        public PrimaryKeyDetails CurrentPrimaryKey { get; set; }
        public PrimaryKeyDetails RequiredPrimaryKey { get; set; }

        public string[] RequiredColumns { get; set; }
        public string[] ColumnsAdded { get; set; }
        public string[] ColumnsRemoved { get; set; }
        public Change<string> RenamedTo { get; set; }
        public Change<OptOut> ClusteredChangedTo { get; set; }
        public bool PrimaryKeyAdded { get; set; }
        public bool PrimaryKeyRemoved { get; set; }
        public Change<string> DescriptionChangedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return ColumnsAdded?.Any() == true
                    || ColumnsRemoved?.Any() == true
                    || PrimaryKeyAdded
                    || PrimaryKeyRemoved
                    || DescriptionChangedTo != null
                    || RenamedTo != null
                    || ClusteredChangedTo != null;
            }
        }
    }
}
