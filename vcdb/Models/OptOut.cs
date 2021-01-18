using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using vcdb.Output;

namespace vcdb.Models
{
    [DebuggerDisplay("OptFor: {value}")]
    [JsonConverter(typeof(OptOutConverter))]
    public class OptOut : IEquatable<OptOut>
    {
        public static readonly OptOut True = new OptOut(true);
        public static readonly OptOut False = new OptOut(false);
        public static readonly OptOut Default = True;

        private readonly bool value;

        public OptOut(bool value)
        {
            this.value = value;
        }

        public static OptOut From(bool? value)
        {
            switch (value)
            {
                case null:
                    return Default;
                case true:
                    return True;
                case false:
                    return False;
            }
        }

        public static explicit operator bool(OptOut option)
        {
            return option.value;
        }

        public static explicit operator OptOut(bool value)
        {
            return value == true
                ? True
                : False;
        }

        public static bool operator ==(bool x, OptOut y)
        {
            return x == (y ?? Default).value;
        }

        public static bool operator !=(bool x, OptOut y)
        {
            return x != (y ?? Default).value;
        }

        public static bool operator ==(OptOut x, bool y)
        {
            return (x ?? Default).value == y;
        }

        public static bool operator !=(OptOut x, bool y)
        {
            return (x ?? Default).value != y;
        }

        public static bool operator ==(OptOut x, OptOut y)
        {
            return (x ?? Default).value == (y ?? Default).value;
        }

        public static bool operator !=(OptOut x, OptOut y)
        {
            return (x ?? Default).value != (y ?? Default).value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public bool Equals(OptOut optOut)
        {
            return (optOut ?? Default).value == value;
        }

        public override bool Equals(object obj)
        {
            return (obj == null && value == Default.value)
                || (obj is bool boolValue && boolValue == value)
                || (obj is OptOut optOut && Equals(optOut));
        }

        public class OptOutConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(OptOut) || objectType == typeof(bool);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = reader.Value as bool?;
                return From(value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var optOut = value as OptOut;
                writer.WriteValue(optOut.value);
            }
        }

        public class OptOutContractResover : IVcdbJsonContractResolver
        {
            public Func<object, bool> GetShouldSerialise(JsonProperty property)
            {
                if (property.PropertyType == typeof(OptOut))
                {
                    return value =>
                    {
                        var optOut = (OptOut)value;
                        return optOut != null && optOut != Default && optOut != True;
                    };
                }

                return null;
            }
        }
    }
}
