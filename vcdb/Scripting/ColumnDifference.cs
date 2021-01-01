using vcdb.Models;

namespace vcdb.Scripting
{
    public class ColumnDifference
    {
        public NamedItem<string, ColumnDetails> CurrentColumn { get; set; }
        public NamedItem<string, ColumnDetails> RequiredColumn { get; set; }
    }
}
