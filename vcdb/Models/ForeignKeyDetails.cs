using System.Collections.Generic;

namespace vcdb.Models
{
    public class ForeignKeyDetails : INamedItem<string>
    {
        /// <summary>
        /// The fully qualified name of the foreign table
        /// </summary>
        public ObjectName ReferencedTable { get; set; }

        /// <summary>
        /// A mapping of columns in this table to the columns in the foreign table for this key
        /// </summary>
        public Dictionary<string, string> Columns { get; set; }

        /// <summary>
        /// The action to perform if the foreign record is deleted
        /// </summary>
        public ForeignActionOption? OnDelete { get; set; }

        /// <summary>
        /// The action to perform if the foreign record is updated
        /// </summary>
        public ForeignActionOption? OnUpdate { get; set; }

        /// <summary>
        /// An optional description for this foreign key
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Any previous names for the foreign key, to indicate whether the foreign key might need to change name
        /// </summary>
        public string[] PreviousNames { get; set; }
    }
}
