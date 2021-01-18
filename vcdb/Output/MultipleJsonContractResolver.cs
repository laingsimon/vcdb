using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace vcdb.Output
{
    public class MultipleJsonContractResolver : DefaultContractResolver
    {
        private readonly IReadOnlyCollection<IVcdbJsonContractResolver> resolvers;

        public MultipleJsonContractResolver(params IVcdbJsonContractResolver[] resolvers)
        {
            this.resolvers = resolvers;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            Func<object, bool> shouldSerialiseProperty = null;

            foreach (var resolver in resolvers)
            {
                var shouldSerialise = resolver.GetShouldSerialise(property);
                if (shouldSerialise == null)
                    continue;

                if (shouldSerialiseProperty == null)
                {
                    shouldSerialiseProperty = shouldSerialise;
                    continue;
                }

                shouldSerialiseProperty = (property) =>
                {
                    return shouldSerialiseProperty(property) && shouldSerialise(property);
                };
            }

            if (shouldSerialiseProperty != null)
            {
                property.ShouldSerialize = (instance) => 
                {
                    var propertyValue = property.ValueProvider.GetValue(instance);
                    return shouldSerialiseProperty(propertyValue);
                };
            }

            return property;
        }
    }
}
