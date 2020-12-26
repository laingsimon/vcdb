using System.Collections.Generic;

namespace vcdb.Models
{
    public class IndexDetails
    {
        public Dictionary<string, IndexColumnDetails> Columns { get; set; }

        public Dictionary<string, IndexColumnDetails> IncludedColumns { get; set; }
        public bool Clustered { get; set; }
    }
}
