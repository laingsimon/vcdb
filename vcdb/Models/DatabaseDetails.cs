using System.Collections.Generic;

namespace vcdb.Models
{
    public class DatabaseDetails
    {
        public Dictionary<TableName, TableDetails> Tables { get; set; }
    }
}
