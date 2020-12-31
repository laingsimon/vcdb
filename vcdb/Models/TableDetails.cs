using System.Collections.Generic;

namespace vcdb.Models
{
    public class TableDetails
    {
        public Dictionary<string, ColumnDetails> Columns { get; set; }
        public Dictionary<string, IndexDetails> Indexes { get; set; }
        public TableName[] PreviousNames { get; set; }
    }
}
