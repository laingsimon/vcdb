using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace vcdb
{
    [TypeConverter(typeof(TypeConverter))]
    [DebuggerDisplay("[{Schema,nq}].[{Name,nq}]")]
    public class ObjectName : IEquatable<ObjectName>
    {
        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public string Schema { get; set; }

        public override int GetHashCode()
        {
            return $"[{Schema}].[{Name}]".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectName);
        }

        public bool Equals(ObjectName other)
        {
            return other != null
                && other.Schema == Schema
                && other.Name == Name;
        }

        public static ObjectName Parse(string tableName)
        {
            var converter = new TypeConverter();
            return (ObjectName)converter.ConvertFrom(null, null, tableName);
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

                return new ObjectName
                {
                    Schema = match.Groups["schema"].Value,
                    Name = match.Groups["table"].Value
                };
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object originalValue, Type destinationType)
            {
                var value = (ObjectName)originalValue;
                var formattedTableName = $"[{value.Schema}].[{value.Name}]";
                return formattedTableName;
            }
        }
    }
}
