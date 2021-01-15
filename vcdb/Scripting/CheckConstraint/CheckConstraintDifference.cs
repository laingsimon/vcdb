using vcdb.Models;

namespace vcdb.Scripting.CheckConstraint
{
    public class CheckConstraintDifference
    {
        public CheckConstraintDetails CurrentConstraint { get; internal set; }
        public CheckConstraintDetails RequiredConstraint { get; set; }
        public bool ConstraintAdded { get; set; }
        public bool ConstraintDeleted { get; set; }
        public Change<string> CheckRenamedTo { get; set; }
        public string CheckChangedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return ConstraintAdded
                    || ConstraintDeleted
                    || CheckRenamedTo != null
                    || CheckChangedTo != null;
            }
        }
    }
}
