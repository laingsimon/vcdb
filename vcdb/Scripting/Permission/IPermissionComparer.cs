using vcdb.Models;

namespace vcdb.Scripting.Permission
{
    public interface IPermissionComparer
    {
        PermissionDifferences GetPermissionDifferences(
            ComparerContext context,
            Permissions currentPermissions,
            Permissions requiredPermissions);
    }
}