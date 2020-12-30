using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace vcdb
{
    [TypeConverter(typeof(TypeConverter))]
    [DebuggerDisplay("[{Schema}].[{Table}]")]
    public class TableName
    {
        [JsonIgnore]
        public string Table { get; set; }

        [JsonIgnore]
        public string Schema { get; set; }

        public class TypeConverter : System.ComponentModel.TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object originalValue)
            {
                var value = (string)originalValue;
                var match = Regex.Match(value, @"^\[?(?<schema>.+?)\]?\.\[?(?<table>.+)\]?$");
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
