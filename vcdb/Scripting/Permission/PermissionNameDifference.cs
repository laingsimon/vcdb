using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting.Permission
{
    public class PermissionNameDifference<TPermissionBreakdown>
    {
        public NamedItem<PermissionName, TPermissionBreakdown> CurrentPermission { get; set; }
        public NamedItem<PermissionName, TPermissionBreakdown> RequiredPermission { get; set; }

        public bool PermissionDeleted { get; set; }
        public bool PermissionAdded { get; set; }
        public IReadOnlyCollection<UserPermissionDifference> UserPermissionDifferences { get; set; }

        public bool IsChanged
        {
            get
            {
                return PermissionAdded
                    || PermissionDeleted
                    || UserPermissionDifferences.Any();
            }
        }

        public static PermissionNameDifference<TPermissionBreakdown> From(KeyValuePair<PermissionName, TPermissionBreakdown> pair)
        {
            return new PermissionNameDifference<TPermissionBreakdown>
            {
                CurrentPermission = null,
                RequiredPermission = pair.AsNamedItem(),
                PermissionAdded = true,
                PermissionDeleted = false
            };
        }
    }
}
