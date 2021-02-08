namespace vcdb.Scripting.ForeignKey
{
    public class ForeignKeyColumnDifference
    {
        public bool ForeignKeyColumnAdded { get; set; }
        public bool ForeignKeyColumnDeleted { get; set; }
        public string RequiredColumnName { get; set; }
        public string CurrentColumnName { get; set; }

        public string RequiredReferencedColumnName { get; set; }
        public string CurrentReferencedColumnName { get; set; }

        public bool IsChanged
        {
            get
            {
                return ForeignKeyColumnAdded
                    || ForeignKeyColumnDeleted
                    || RequiredReferencedColumnName != CurrentReferencedColumnName; //TODO: Check that this handles renames in the referenced table
            }
        }
    }
}
