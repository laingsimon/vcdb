using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace vcdb.Models
{
    /// <summary>
    /// A class that represents a permission by name, e.g. CONTROL or SELECT
    /// This can be used in a GRANT, REVOKE or DENY permission classification
    /// </summary>
    [DebuggerDisplay("{name,nq}")]
    [TypeConverter(typeof(TypeConverter))]
    public class PermissionName
    {
        private readonly string name;

        public PermissionName(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            return obj is PermissionName name &&
                   this.name == name.name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name);
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
                return new PermissionName(value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object originalValue, Type destinationType)
            {
                var value = (PermissionName)originalValue;
                return value.name;
            }
        }
    }
}
