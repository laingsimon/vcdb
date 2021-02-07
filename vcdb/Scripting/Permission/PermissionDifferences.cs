using System;
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

        public PermissionDifferences MergeIn(PermissionDifferences other)
        {
            if (other == null)
            {
                return this;
            }

            return new PermissionDifferences
            {
                CurrentPermissions = CurrentPermissions,
                RequiredPermissions = RequiredPermissions,
                DenyChanges = MergeIn(DenyChanges, other.DenyChanges),
                GrantChanges = MergeIn(GrantChanges, other.GrantChanges),
                RevokeChanges = MergeIn(RevokeChanges, other.RevokeChanges)
            };
        }

        private static IReadOnlyCollection<PermissionNameDifference<HashSet<UserPrincipal>>> MergeIn(
            IReadOnlyCollection<PermissionNameDifference<HashSet<UserPrincipal>>> current,
            IReadOnlyCollection<PermissionNameDifference<HashSet<UserPrincipal>>> other)
        {
            if (current == null || other == null)
            {
                return current ?? other;
            }

            return current.Concat(other).ToArray();
        }

        private static IReadOnlyCollection<PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>>> MergeIn(
            IReadOnlyCollection<PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>>> current,
            IReadOnlyCollection<PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>>> other)
        {
            if (current == null || other == null)
            {
                return current ?? other;
            }

            return current.Concat(other).ToArray();
        }
    }
}
