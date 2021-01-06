﻿using System.Collections.Generic;

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
    }
}