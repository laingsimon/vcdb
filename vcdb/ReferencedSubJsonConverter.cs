using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace vcdb
{
    /// <summary>
    /// This converter will deserialise a json object from a file if it is expressed as a string in the JSON file.
    /// 
    /// <code>
    /// {
    ///   "objectProperty": { ... },
    ///   "arrayProperty": [ ... ]
    /// }
    /// </code>
    /// 
    /// can be expressed as:
    /// 
    /// <code>
    /// {
    ///   "objectProperty": "objectDetails.json",
    ///   "arrayProperty": "arrayContents.json"
    /// }
    /// 
    /// NOTE: This object is NOT thread-safe.
    /// </code>
    /// </summary>
    public class ReferencedSubJsonConverter : JsonConverter
    {
        private readonly AutoResetState enabled = new AutoResetState();
        private readonly string workingPath;

        public ReferencedSubJsonConverter(string workingPath)
        {
            this.workingPath = workingPath;
        }

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return enabled.GetStateAndEnable()
                && objectType.IsClass 
                && objectType != typeof(string)
                && objectType != typeof(object)
                && !IsVcdbObjectRepresentationWithCustomConverter(objectType);
        }

        private static bool IsVcdbObjectRepresentationWithCustomConverter(Type objectType)
        {
            var namespacePrefix = typeof(Program).Namespace;
            return objectType.Namespace.StartsWith(namespacePrefix)
                && objectType.GetCustomAttributes(true).Any(a => a is TypeConverterAttribute || a is JsonConverterAttribute);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string filePath)
            {
                var fullPath = Path.Combine(workingPath, filePath);
                using (var subJsonReader = new JsonTextReader(new StreamReader(fullPath)))
                {
                    return serializer.Deserialize(subJsonReader, objectType);
                }
            }

            enabled.Disable(); //temporarily use default deserialisation (by disabling this converter)
            return serializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        private class AutoResetState
        {
            private bool enabled;

            /// <summary>
            /// Get the current state, ensure the status is left as enabled
            /// </summary>
            /// <returns></returns>
            public bool GetStateAndEnable()
            {
                if (enabled)
                    return true;
                enabled = true;
                return false;
            }

            /// <summary>
            /// Set the status to disabled
            /// </summary>
            public void Disable()
            {
                if (!enabled)
                    throw new NotSupportedException("Already disabled");

                enabled = false;
            }
        }
    }
}
