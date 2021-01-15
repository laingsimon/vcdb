using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace vcdb.Output
{
    public class JsonOutputContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && !typeof(string).IsAssignableFrom(property.PropertyType))
            {
                var shouldSerialiseFallback = property.ShouldSerialize ?? (_ => true);
                property.ShouldSerialize =
                    instance => ShouldSerialiseProperty(instance, property) && shouldSerialiseFallback(instance);
            }
            
            return property;
        }

        private bool ShouldSerialiseProperty(object instance, JsonProperty property)
        {
            var value = property.ValueProvider.GetValue(instance);
            if (value is ICollection collection && collection.Count == 0)
                return collection.Count > 0;

            if (value is IEnumerable)
            {
                var countProperty = value.GetType().GetProperty(nameof(ICollection.Count));
                var count = (int?)countProperty?.GetValue(value);
                return count == null || count > 0;
            }

            return true;
        }
    }
}
