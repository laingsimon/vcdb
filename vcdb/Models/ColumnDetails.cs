﻿namespace vcdb.Models
{
    public class ColumnDetails
    {
        public string Type { get; set; }
        public bool? Nullable { get; set; }
        public object Default { get; set; }
        public string[] PreviousNames { get; set; }
    }
}
