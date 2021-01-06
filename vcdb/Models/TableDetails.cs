using System.Collections.Generic;

namespace vcdb.Models
{
    public class TableDetails : INamedItem<TableName>
    {
        public Dictionary<string, ColumnDetails> Columns { get; set; }
        public Dictionary<string, IndexDetails> Indexes { get; set; }
        public TableName[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the table
        /// </summary>
        public string Description { get; set; }
    }
}
