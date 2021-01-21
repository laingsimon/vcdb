using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace vcdb
{
    [TypeConverter(typeof(TypeConverter))]
    [DebuggerDisplay("[{Schema,nq}].[{Table,nq}]")]
    public class TableName : IEquatable<TableName>
    {
        [JsonIgnore]
        public string Table { get; set; }

        [JsonIgnore]
        public string Schema { get; set; }

        public override int GetHashCode()
        {
            return $"[{Schema}].[{Table}]".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TableName);
        }

        public bool Equals(TableName other)
        {
            return other != null
                && other.Schema == Schema
                && other.Table == Table;
        }

        public static TableName Parse(string tableName)
        {
            var converter = new TypeConverter();
            return (TableName)converter.ConvertFrom(null, null, tableName);
        }

        public class TypeConverter : System.ComponentModel.TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object originalValue)
            {
                var value = (string)originalValue;
                var match = Regex.Match(value, @"^\[?(?<schema>.+?)\]?\.\[?(?<table>.+?)\]?$");
                if (!match.Success)
                    throw new FormatException($"TableName `{value}` is not a valid table name");

                return new TableName
                {
                    Schema = match.Groups["schema"].Value,
                    Table = match.Groups["table"].Value
                };
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object originalValue, Type destinationType)
            {
                var value = (TableName)originalValue;
                var formattedTableName = $"[{value.Schema}].[{value.Table}]";
                return formattedTableName;
            }
        }
    }
}
