using System.Collections.Generic;

namespace vcdb.Models
{
    public class DatabaseDetails
    {
        public Dictionary<TableName, TableDetails> Tables { get; set; }
        public Dictionary<string, SchemaDetails> Schemas { get; set; } = new Dictionary<string, SchemaDetails>();
        public string Description { get; set; }
    }
}
