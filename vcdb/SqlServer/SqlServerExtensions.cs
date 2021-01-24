using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.SqlServer
{
    public static class SqlServerExtensions
    {
        public static string SqlSafeName(this TableName tableName)
        {
            return $"{tableName.Schema.SqlSafeName()}.{tableName.Table.SqlSafeName()}";
        }

        public static string SqlSafeName(this string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                return null;

            return $"[{columnName}]";
        }

        public static string SqlSafeName<TValue>(this NamedItem<string, TValue> namedItem)
        {
            return namedItem.Key.SqlSafeName();
        }

        public static string SqlSafeName<TValue>(this KeyValuePair<string, TValue> namedItem)
        {
            return namedItem.Key.SqlSafeName();
        }

        public static string SqlSafeName(this UserPrincipal userPrincipal)
        {
            return $"[{userPrincipal.name}]";
        }

        public static string SqlSafeName(this PermissionName permissionName)
        {
            return permissionName.name;
        }
    }
}
