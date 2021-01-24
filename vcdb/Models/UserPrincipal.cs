using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace vcdb.Models
{
    /// <summary>
    /// A class that represents a user (or other principal) within a database
    /// </summary>
    [DebuggerDisplay("{name,nq}")]
    [TypeConverter(typeof(TypeConverter))]
    public class UserPrincipal
    {
        internal readonly string name;

        public UserPrincipal(string name)
        {
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            return obj is UserPrincipal name &&
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
                return new UserPrincipal(value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object originalValue, Type destinationType)
            {
                var value = (UserPrincipal)originalValue;
                return value.name;
            }
        }
    }
}
