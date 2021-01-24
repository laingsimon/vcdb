using System;
using vcdb.Models;

namespace vcdb.Scripting.Permission
{
    public class UserPermissionDifference
    {
        public NamedItem<UserPrincipal, PermissionDetails> CurrentPermission { get; set; }
        public NamedItem<UserPrincipal, PermissionDetails> RequiredPermission { get; set; }
        public bool PermissionAdded { get; set; }
        public bool PermissionDeleted { get; set; }
        public Change<bool> WithGrantChangedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return PermissionAdded
                    || PermissionDeleted
                    || WithGrantChangedTo != null;
            }
        }
    }
}
