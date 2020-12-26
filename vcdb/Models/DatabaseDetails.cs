using System.Collections.Generic;

namespace vcdb.Models
{
    public class DatabaseDetails
    {
        public Dictionary<string, TableDetails> Tables { get; set; }
    }
}
