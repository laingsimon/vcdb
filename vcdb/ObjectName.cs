using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace vcdb
{
    [TypeConverter(typeof(TypeConverter))]
    [DebuggerDisplay("{DebugString(),nq}")]
    public class ObjectName : IEquatable<ObjectName>
    {
        /// <summary>
        /// This converter controls how names are formatted in the JSON file, also how they are then read from the JSON file.
        /// To use a different format, replace the NameConverter here with a different instance
        /// </summary>
        public static IObjectNameConverter Converter = new ObjectNameConverter();

        /// <summary>
        /// This is the name converter that should be used for THIS INSTANCE, it must not change for the life of the object as otherwise the HashCode could change.
        /// </summary>
        internal readonly IObjectNameConverter nameConverter; 

        public ObjectName()
            :this(null)
        { }

        public ObjectName(IObjectNameConverter nameConverter = null)
        {
            this.nameConverter = nameConverter ?? Converter;
        }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public string Schema { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ (Schema?.GetHashCode() ?? 0);
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

        internal string DebugString()
        {
            return nameConverter.ConvertToString(Schema, Name);
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
                var match = Converter.ExtractFromString(value);
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
                var formattedTableName = Converter.ConvertToString(value.Schema, value.Name);
                return formattedTableName;
            }
        }
    }
}
