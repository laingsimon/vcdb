using Newtonsoft.Json;

namespace vcdb.Models
{
    public class ColumnDetails : INamedItem<string>
    {
        /// <summary>
        /// The data type of the column
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Whether the column should accept nulls
        /// </summary>
        public OptOut Nullable { get; set; }

        /// <summary>
        /// The default value for the column
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// A optional name for the default to add to the table
        /// </summary>
        public string DefaultName { get; set; }

        /// <summary>
        /// For internal use only, the current name of the sql default if one is present
        /// </summary>
        [JsonIgnore]
        public string SqlDefaultName { get; set; }

        /// <summary>
        /// For internal use only, the id of the default constraint - if present
        /// </summary>
        [JsonIgnore]
        public int? DefaultObjectId { get; set; }

        /// <summary>
        /// Any previous names for the column, to indicate whether the column might need to change name
        /// </summary>
        public string[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the column
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If appropriate, the collation that should be used for the column
        /// If null the column should inherit the collation of the database
        /// </summary>
        public string Collation { get; set; }

        /// <summary>
        /// Whether this column is part of the primary key for this table
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// The user defined permissions on this column
        /// </summary>
        public Permissions Permissions { get; set; }
    }
}
