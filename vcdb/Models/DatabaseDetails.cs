using Newtonsoft.Json;
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

        /// <summary>
        /// The collation for the database, null means: use the server collation
        /// </summary>
        public string Collation { get; set; }

        /// <summary>
        /// For internal use, the collation of the server
        /// </summary>
        [JsonIgnore]
        public string ServerCollation { get; set; }

        /// <summary>
        /// The users permitted to access this database
        /// </summary>
        public Dictionary<string, UserDetails> Users { get; set; }

        /// <summary>
        /// The permissions assigned to users in this database
        /// </summary>
        public Permissions Permissions { get; set; }
    }
}
