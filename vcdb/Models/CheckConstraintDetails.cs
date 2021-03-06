﻿using Newtonsoft.Json;
using System.Diagnostics;

namespace vcdb.Models
{
    [DebuggerDisplay("{Check,nq}")]
    public class CheckConstraintDetails
    {
        /// <summary>
        /// An optional name for this check constraint
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Any previous names for the constraint, to indicate whether the constraint might need to change name
        /// </summary>
        public string[] PreviousNames { get; set; }

        /// <summary>
        /// The enforced check definition
        /// </summary>
        public string Check { get; set; }

        /// <summary>
        /// For internal use only, the actual name of the constraint
        /// </summary>
        [JsonIgnore]
        public string SqlName { get; set; }

        /// <summary>
        /// For internal use only, the id of the default constraint - if present
        /// </summary>
        [JsonIgnore]
        public int? CheckObjectId { get; set; }

        /// <summary>
        /// For internal use only, the column the check constraint is bound to (if any)
        /// NOTE: There may be multiple columns
        /// </summary>
        [JsonIgnore]
        public string[] ColumnNames { get; set; }
    }
}
