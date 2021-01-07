using System.Collections.Generic;

namespace vcdb.Models
{
    public class IndexDetails : INamedItem<string>
    {
        /// <summary>
        /// The columns in the index
        /// </summary>
        public Dictionary<string, IndexColumnDetails> Columns { get; set; }

        /// <summary>
        /// The additional columns that are included in the index
        /// </summary>
        public IReadOnlyCollection<string> Including { get; set; }

        /// <summary>
        /// Whether the index is clustered
        /// </summary>
        public bool Clustered { get; set; }

        /// <summary>
        /// Whether the index contains unique values
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// Any previous names for the index, to indicate whether the index might need to change name
        /// </summary>
        public string[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the index
        /// </summary>
        public string Description { get; set; }
    }
}
