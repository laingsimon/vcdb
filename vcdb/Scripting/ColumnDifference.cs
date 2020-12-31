using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class ColumnDifference
    {
        public KeyValuePair<string, ColumnDetails>? CurrentColumn { get; set; }
        public KeyValuePair<string, ColumnDetails>? RequiredColumn { get; set; }
    }
}
