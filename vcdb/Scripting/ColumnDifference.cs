using System;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class ColumnDifference
    {
        public static readonly object Unchanged = new object();

        public NamedItem<string, ColumnDetails> CurrentColumn { get; set; }
        public NamedItem<string, ColumnDetails> RequiredColumn { get; set; }

        public string ColumnRenamedTo { get; set; }
        public string TypeChangedTo { get; set; }
        public bool? NullabilityChangedTo { get; set; }
        public object DefaultChangedTo { get; set; } = Unchanged;
        public bool ColumnAdded { get; set; }
        public bool ColumnDeleted { get; set; }
        public string DefaultRenamedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return ColumnRenamedTo != null
                    || TypeChangedTo != null
                    || NullabilityChangedTo != null
                    || DefaultHasChanged
                    || ColumnAdded
                    || ColumnDeleted
                    || DefaultRenamedTo != null;
            }
        }

        public bool DefaultHasChanged
        {
            get
            {
                return !ReferenceEquals(DefaultChangedTo, Unchanged);
            }
        }

        public  ColumnDifference MergeIn(ColumnDifference other)
        {
            return new ColumnDifference
            {
                CurrentColumn = CurrentColumn ?? other.CurrentColumn,
                RequiredColumn = RequiredColumn ?? other.RequiredColumn,
                ColumnRenamedTo = ColumnRenamedTo ?? other.ColumnRenamedTo,
                ColumnAdded = ColumnAdded || other.ColumnAdded,
                ColumnDeleted = ColumnDeleted || other.ColumnDeleted,
                TypeChangedTo = TypeChangedTo ?? other.TypeChangedTo,
                NullabilityChangedTo = NullabilityChangedTo ?? other.NullabilityChangedTo,
                DefaultChangedTo = GetDefaultsChangedTo() ?? other.GetDefaultsChangedTo() ?? Unchanged,
                DefaultRenamedTo = DefaultRenamedTo ?? other.DefaultRenamedTo
            };
        }

        private object GetDefaultsChangedTo()
        {
            return DefaultChangedTo == Unchanged
                ? null
                : DefaultChangedTo;
        }
    }
}
