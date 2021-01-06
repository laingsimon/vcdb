using vcdb.Models;

namespace vcdb.Scripting
{
    public class ColumnDifference
    {
        public static readonly object UnchangedDefault = new object();
        public const string UnchangedDescription = "\0";

        public NamedItem<string, ColumnDetails> CurrentColumn { get; set; }
        public NamedItem<string, ColumnDetails> RequiredColumn { get; set; }

        public string ColumnRenamedTo { get; set; }
        public string TypeChangedTo { get; set; }
        public bool? NullabilityChangedTo { get; set; }
        public object DefaultChangedTo { get; set; } = UnchangedDefault;
        public bool ColumnAdded { get; set; }
        public bool ColumnDeleted { get; set; }
        public string DefaultRenamedTo { get; set; }
        public string DescriptionChangedTo { get; set; }

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
                    || DefaultRenamedTo != null
                    || DescriptionHasChanged;
            }
        }

        public bool DefaultHasChanged
        {
            get
            {
                return !ReferenceEquals(DefaultChangedTo, UnchangedDefault);
            }
        }

        public bool DescriptionHasChanged
        {
            get
            {
                return DescriptionChangedTo != UnchangedDescription;
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
                DefaultChangedTo = GetDefaultChangedTo() ?? other.GetDefaultChangedTo() ?? UnchangedDefault,
                DefaultRenamedTo = DefaultRenamedTo ?? other.DefaultRenamedTo,
                DescriptionChangedTo = GetDescriptionChangedTo() ?? other.GetDescriptionChangedTo() ?? UnchangedDescription
            };
        }

        private object GetDefaultChangedTo()
        {
            return DefaultHasChanged
                ? DefaultChangedTo
                : null;
        }

        private string GetDescriptionChangedTo()
        {
            return DescriptionHasChanged
                ? DescriptionChangedTo
                : null;
        }
    }
}
