using Newtonsoft.Json;

namespace vcdb.Models
{
    public class PrimaryKeyDetails
    {
        /// <summary>
        /// An optional name for the primary key
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Any previous names, if applicable, for the name of the primary key - to indicate whether the primary key has changed name
        /// </summary>
        public string[] PreviousNames { get; set; }

        /// <summary>
        /// Whether the index for this primary key should contain unique values or not
        /// </summary>
        public bool? Unique { get; set; }

        /// <summary>
        /// Whether the index for this primary key should be clustered or not
        /// </summary>
        [JsonIgnore()]
        public bool Clustered { get; set; } = true;

        /// <summary>
        /// A description for the table
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// For internal use only, the id of the primary key - if present
        /// </summary>
        [JsonIgnore]
        public int ObjectId { get; set; }

        /// <summary>
        /// For internal use only, the actual name of the constraint
        /// </summary>
        [JsonIgnore]
        public string SqlName { get; set; }

        [JsonProperty(nameof(Clustered))]
        public bool? JsonClustered
        {
            get
            {
                if (Clustered)
                    return null; //the default value for a nullable-bool

                return false;
            }
            set
            {
                if (value == null)
                    return;

                Clustered = value.Value;
            }
        }
    }
}
