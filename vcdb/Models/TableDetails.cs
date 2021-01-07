using System.Collections.Generic;

namespace vcdb.Models
{
    public class TableDetails : INamedItem<TableName>
    {
        /// <summary>
        /// The columns in the table
        /// </summary>
        public Dictionary<string, ColumnDetails> Columns { get; set; }

        /// <summary>
        /// The indexes bound to the table
        /// </summary>
        public Dictionary<string, IndexDetails> Indexes { get; set; }

        /// <summary>
        /// Any previous names for the table, to indicate whether the table might need to change name
        /// </summary>
        public TableName[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the table
        /// </summary>
        public string Description { get; set; }
    }
}
