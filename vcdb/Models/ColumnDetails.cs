﻿using Newtonsoft.Json;

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
        public bool? Nullable { get; set; }

        /// <summary>
        /// The default value for the column
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// A optional name for the default to add to the table
        /// </summary>
        public string DefaultName { get; set; }

        /// <summary>
        /// For internal use only, the id of the default constraint - if present
        /// </summary>
        [JsonIgnore]
        internal int? DefaultObjectId { get; set; }

        /// <summary>
        /// For internal use only, the id of the default constraint - if present
        /// </summary>
        [JsonIgnore]
        internal int? CheckObjectId { get; set; }

        /// <summary>
        /// Any previous names for the column, to indicate whether the column might need to change name
        /// </summary>
        public string[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the column
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The check constraint for the column
        /// </summary>
        public string Check { get; set; }

        /// <summary>
        /// A optional name for the check to add to the table
        /// </summary>
        public string CheckName { get; set; }
    }
}
