using System.Collections.Generic;

namespace vcdb.Models
{
    public class TableDetails : INamedItem<ObjectName>
    {
        /// <summary>
        /// The columns in the table
        /// </summary>
        public IDictionary<string, ColumnDetails> Columns { get; set; }

        /// <summary>
        /// The indexes bound to the table
        /// </summary>
        public IDictionary<string, IndexDetails> Indexes { get; set; }

        /// <summary>
        /// Any check constraints bound to columns on this table
        /// </summary>
        public CheckConstraintDetails[] Checks { get; set; }

        /// <summary>
        /// Any previous names for the table, to indicate whether the table might need to change name
        /// </summary>
        public ObjectName[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the table
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The details of the primary key on this table
        /// The columns in the primary key are identified by their 'primaryKey' property
        /// </summary>
        public PrimaryKeyDetails PrimaryKey { get; set; }

        /// <summary>
        /// The user defined permissions for this table
        /// </summary>
        public Permissions Permissions { get; set; }
    }
}
