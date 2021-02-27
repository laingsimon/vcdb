using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Linq;
using vcdb.Models;

namespace vcdb.Output
{
    public class JsonOutputContractResolver : IVcdbJsonContractResolver
    {
        public Func<object, bool> GetShouldSerialise(JsonProperty property)
        {
            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && !typeof(string).IsAssignableFrom(property.PropertyType))
            {
                return ShouldSerialiseProperty;
            }

            if (typeof(Permissions).IsAssignableFrom(property.PropertyType))
            {
                return ShouldSerialiseProperty;
            }

            return null;
        }

        private bool ShouldSerialiseProperty(object value)
        {
            if (value is ICollection collection && collection.Count == 0)
                return collection.Count > 0;

            if (value is IEnumerable)
            {
                var countProperty = value.GetType().GetProperty(nameof(ICollection.Count));
                var count = (int?)countProperty?.GetValue(value);
                return count == null || count > 0;
            }

            if (value is Permissions permissions)
            {
                return permissions.Deny?.Any() == true
                    || permissions.Grant?.Any() == true
                    || permissions.Revoke?.Any() == true
                    || permissions.SubEntityPermissions != null;
            }

            return true;
        }
    }
}
