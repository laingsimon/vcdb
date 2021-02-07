using vcdb.Models;
using vcdb.Scripting.Permission;

namespace vcdb.Scripting.Column
{
    public class ColumnDifference
    {
        public NamedItem<string, ColumnDetails> CurrentColumn { get; set; }
        public NamedItem<string, ColumnDetails> RequiredColumn { get; set; }

        public string ColumnRenamedTo { get; set; }
        public string TypeChangedTo { get; set; }
        public Change<OptOut> NullabilityChangedTo { get; set; }
        public Change<object> DefaultChangedTo { get; set; }

        public bool ColumnAdded { get; set; }
        public bool ColumnDeleted { get; set; }
        public Change<string> DefaultRenamedTo { get; set; }
        public Change<string> DescriptionChangedTo { get; set; }
        public string CollationChangedTo { get; set; }
        public PermissionDifferences PermissionDifferences { get; set; }
        public string ExpressionChangedTo { get; set; }
        public Change<bool> ComputedChangedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return ColumnRenamedTo != null
                    || TypeChangedTo != null
                    || NullabilityChangedTo != null
                    || DefaultChangedTo != null
                    || ColumnAdded
                    || ColumnDeleted
                    || DefaultRenamedTo != null
                    || DescriptionChangedTo != null
                    || CollationChangedTo != null
                    || PermissionDifferences != null
                    || ExpressionChangedTo != null
                    || ComputedChangedTo != null;
            }
        }

        public ColumnDifference MergeIn(ColumnDifference other)
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
                DefaultChangedTo = DefaultChangedTo ?? other.DefaultChangedTo,
                DefaultRenamedTo = DefaultRenamedTo ?? other.DefaultRenamedTo,
                DescriptionChangedTo = DescriptionChangedTo ?? other.DescriptionChangedTo,
                CollationChangedTo = CollationChangedTo ?? other.CollationChangedTo,
                ExpressionChangedTo = ExpressionChangedTo ?? other.ExpressionChangedTo,
                ComputedChangedTo = ComputedChangedTo ?? other.ComputedChangedTo,
                PermissionDifferences = PermissionDifferences?.MergeIn(other.PermissionDifferences) ?? other.PermissionDifferences
            };
        }
    }
}
