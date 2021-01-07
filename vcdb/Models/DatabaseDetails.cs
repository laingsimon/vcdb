using System.Collections.Generic;

namespace vcdb.Models
{
    public class DatabaseDetails
    {
        /// <summary>
        /// The tables in the database
        /// </summary>
        public Dictionary<TableName, TableDetails> Tables { get; set; }

        /// <summary>
        /// The tables in the database
        /// </summary>
        public Dictionary<string, SchemaDetails> Schemas { get; set; }

        /// <summary>
        /// A description of the database
        /// </summary>
        public string Description { get; set; }
    }
}
