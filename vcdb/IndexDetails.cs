using Newtonsoft.Json;
using System.Collections.Generic;

namespace vcdb
{
    public class IndexDetails
    {
        public Dictionary<string, IndexColumnDetails> Columns { get; set; }

        public Dictionary<string, IndexColumnDetails> IncludedColumns { get; set; }
        public bool Clustered { get; set; }
    }
}
