using System.Collections.Generic;

namespace vcdb
{
    public class TableDetails
    {
        public Dictionary<string, ColumnDetails> Columns { get; set; }
        public Dictionary<string, IndexDetails> Indexes { get; set; }
    }
}
