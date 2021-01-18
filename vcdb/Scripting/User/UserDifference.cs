using vcdb.Models;

namespace vcdb.Scripting.User
{
    public class UserDifference
    {
        public NamedItem<string, UserDetails> RequiredUser { get; set; }
        public NamedItem<string, UserDetails> CurrentUser { get; set; }

        public bool UserAdded { get; set; }
        public bool UserDeleted { get; set; }
        public string UserRenamedTo { get; set; }
        public Change<bool> StateChangedTo { get; set; }
        public string LoginChangedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return UserAdded
                    || UserDeleted
                    || UserRenamedTo != null
                    || StateChangedTo != null
                    || LoginChangedTo != null;
            }
        }
    }
}
