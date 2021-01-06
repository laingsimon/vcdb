using System.Collections.Generic;

namespace vcdb.Models
{
    public class IndexDetails : INamedItem<string>
    {
        public Dictionary<string, IndexColumnDetails> Columns { get; set; }
        public IReadOnlyCollection<string> Including { get; set; }
        public bool Clustered { get; set; }
        public bool Unique { get; set; }

        public string[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the index
        /// </summary>
        public string Description { get; set; }
    }
}
