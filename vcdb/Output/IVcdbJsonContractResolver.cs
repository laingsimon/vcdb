using Newtonsoft.Json.Serialization;
using System;

namespace vcdb.Output
{
    public interface IVcdbJsonContractResolver
    {
        Func<object, bool> GetShouldSerialise(JsonProperty property);
    }
}
