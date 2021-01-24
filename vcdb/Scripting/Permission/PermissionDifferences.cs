using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting.Permission
{
    public class PermissionDifferences
    {
        public Permissions CurrentPermissions { get; set; }
        public Permissions RequiredPermissions { get; set; }
        public IReadOnlyCollection<PermissionNameDifference<HashSet<UserPrincipal>>> DenyChanges { get; set; }
        public IReadOnlyCollection<PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>>> GrantChanges { get; set; }
        public IReadOnlyCollection<PermissionNameDifference<HashSet<UserPrincipal>>> RevokeChanges { get; set; }

        public bool IsChanged
        {
            get
            {
                return DenyChanges.Any()
                    || GrantChanges.Any()
                    || RevokeChanges.Any();
            }
        }

        public static PermissionDifferences From(Permissions permissions)
        {
            if (permissions == null)
                return null;

            return new PermissionDifferences
            {
                DenyChanges = permissions.Deny?.Select(pair => PermissionNameDifference<HashSet<UserPrincipal>>.From(pair)).ToArray(),
                GrantChanges = permissions.Grant?.Select(pair => PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>>.From(pair)).ToArray(),
                RevokeChanges = permissions.Revoke?.Select(pair => PermissionNameDifference<HashSet<UserPrincipal>>.From(pair)).ToArray()
            };
        }
    }
}
