using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting.Permission
{
    public class PermissionComparer : IPermissionComparer
    {
        public PermissionDifferences GetPermissionDifferences(
            ComparerContext context,
            Permissions currentPermissions,
            Permissions requiredPermissions)
        {
            var permissionDifference = new PermissionDifferences
            {
                CurrentPermissions = currentPermissions,
                RequiredPermissions = requiredPermissions,
                DenyChanges = GetPermissionTypeDifferences(
                    context,
                    currentPermissions?.Deny ?? new Dictionary<PermissionName, HashSet<UserPrincipal>>(),
                    requiredPermissions?.Deny ?? new Dictionary<PermissionName, HashSet<UserPrincipal>>(),
                    GetUserPermissionDifferences)
                    .ToArray(),
                GrantChanges = GetPermissionTypeDifferences(
                    context,
                    currentPermissions?.Grant ?? new Dictionary<PermissionName, Dictionary<UserPrincipal, PermissionDetails>>(),
                    requiredPermissions?.Grant ?? new Dictionary<PermissionName, Dictionary<UserPrincipal, PermissionDetails>>(),
                    GetUserPermissionDifferences)
                    .ToArray(),
                RevokeChanges = GetPermissionTypeDifferences(
                    context,
                    currentPermissions?.Revoke ?? new Dictionary<PermissionName, HashSet<UserPrincipal>>(),
                    requiredPermissions?.Revoke ?? new Dictionary<PermissionName, HashSet<UserPrincipal>>(),
                    GetUserPermissionDifferences)
                    .ToArray()
            };

            return permissionDifference.IsChanged
                ? permissionDifference
                : null;
        }

        private IEnumerable<PermissionNameDifference<TPermissionBreakdown>> GetPermissionTypeDifferences<TPermissionBreakdown>(
            ComparerContext context, 
            Dictionary<PermissionName, TPermissionBreakdown> currentPermissions,
            Dictionary<PermissionName, TPermissionBreakdown> requiredPermissions,
            Func<TPermissionBreakdown, TPermissionBreakdown, IEnumerable<UserPermissionDifference>> getUserPermissionDifferences)
        {
            var processedPermissions = new HashSet<TPermissionBreakdown>();
            foreach (var requiredPermission in requiredPermissions)
            {
                var currentPermission = currentPermissions.ItemOrDefault(requiredPermission.Key);

                if (currentPermission == null)
                {
                    yield return new PermissionNameDifference<TPermissionBreakdown>
                    {
                        RequiredPermission = requiredPermission.AsNamedItem(),
                        PermissionAdded = true
                    };
                }
                else
                {
                    processedPermissions.Add(currentPermission);

                    var difference = new PermissionNameDifference<TPermissionBreakdown>
                    {
                        CurrentPermission = new NamedItem<PermissionName, TPermissionBreakdown>(requiredPermission.Key, currentPermission),
                        RequiredPermission = requiredPermission.AsNamedItem(),
                        UserPermissionDifferences = getUserPermissionDifferences(currentPermission, requiredPermission.Value).ToArray()
                        //TODO: Child permission changes
                    };

                    if (difference.IsChanged)
                    {
                        yield return difference;
                    }
                }
            }

            foreach (var currentPermission in currentPermissions.Where(col => !processedPermissions.Contains(col.Value)))
            {
                yield return new PermissionNameDifference<TPermissionBreakdown>
                {
                    CurrentPermission = currentPermission.AsNamedItem(),
                    PermissionDeleted = true
                };
            }
        }

        private IEnumerable<UserPermissionDifference> GetUserPermissionDifferences(
            Dictionary<UserPrincipal, PermissionDetails> currentPermissions,
            Dictionary<UserPrincipal, PermissionDetails> requiredPermissions)
        {
            var processedPermissions = new HashSet<PermissionDetails>();
            foreach (var requiredPermission in requiredPermissions)
            {
                var currentPermission = GetCurrentItem(currentPermissions, requiredPermission.Key);

                if (currentPermission == null)
                {
                    yield return new UserPermissionDifference
                    {
                        RequiredPermission = requiredPermission.AsNamedItem(),
                        PermissionAdded = true
                    };
                }
                else
                {
                    processedPermissions.Add(currentPermission.Value);

                    var difference = new UserPermissionDifference
                    {
                        CurrentPermission = currentPermission,
                        RequiredPermission = requiredPermission.AsNamedItem(),
                        WithGrantChangedTo = currentPermission.Value.WithGrant != requiredPermission.Value.WithGrant
                            ? requiredPermission.Value.WithGrant.AsChange()
                            : null
                    };

                    if (difference.IsChanged)
                    {
                        yield return difference;
                    }
                }
            }

            foreach (var currentPermission in currentPermissions.Where(col => !processedPermissions.Contains(col.Value)))
            {
                yield return new UserPermissionDifference
                {
                    CurrentPermission = currentPermission.AsNamedItem(),
                    PermissionDeleted = true
                };
            }
        }

        private IEnumerable<UserPermissionDifference> GetUserPermissionDifferences(
            HashSet<UserPrincipal> currentPrincipals,
            HashSet<UserPrincipal> requiredPrincipals)
        {
            var processedPrincipals = new HashSet<UserPrincipal>();
            foreach (var requiredPrincipal in requiredPrincipals)
            {
                var currentPrincipal = GetCurrentItem(currentPrincipals, requiredPrincipal);

                if (currentPrincipal == null)
                {
                    yield return new UserPermissionDifference
                    {
                        RequiredPermission = new NamedItem<UserPrincipal, PermissionDetails>(requiredPrincipal, null),
                        PermissionAdded = true
                    };
                }
            }

            foreach (var currentPrincipal in currentPrincipals.Where(col => !processedPrincipals.Contains(col)))
            {
                yield return new UserPermissionDifference
                {
                    CurrentPermission = new NamedItem<UserPrincipal, PermissionDetails>(currentPrincipal, null),
                    PermissionDeleted = true
                };
            }
        }

        private NamedItem<UserPrincipal, PermissionDetails> GetCurrentItem(Dictionary<UserPrincipal, PermissionDetails> currentPermissions, UserPrincipal requiredPrincipal)
        {
            //TODO: Handle user renames

            return currentPermissions.ContainsKey(requiredPrincipal)
                ? new NamedItem<UserPrincipal, PermissionDetails>(requiredPrincipal, currentPermissions[requiredPrincipal])
                : null;
        }

        private UserPrincipal GetCurrentItem(HashSet<UserPrincipal> currentPrincipals, UserPrincipal requiredPrincipal)
        {
            //TODO: Handle user renames

            return currentPrincipals.SingleOrDefault(p => p.Equals(requiredPrincipal));
        }
    }
}
