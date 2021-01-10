﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TestFramework
{
    internal class Json : IJson
    {
        private readonly JsonSerializer jsonSerializer;
        private readonly DirectoryInfo scenarioDirectory;

        public Json(DirectoryInfo scenarioDirectory, JsonSerializer jsonSerializer)
        {
            this.scenarioDirectory = scenarioDirectory;
            this.jsonSerializer = jsonSerializer;
        }

        public JToken ReadJsonFromFile(string scenarioRelativePath)
        {
            using (var reader = new StreamReader(Path.Combine(scenarioDirectory.FullName, scenarioRelativePath)))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return (JToken)jsonSerializer.Deserialize<object>(jsonReader);
            }
        }

        public T ReadJsonFromFile<T>(string scenarioRelativePath)
        {
            using (var reader = new StreamReader(Path.Combine(scenarioDirectory.FullName, scenarioRelativePath)))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return jsonSerializer.Deserialize<T>(jsonReader);
            }
        }

        public JToken ReadJsonContent(string json)
        {
            using (var reader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return (JToken)jsonSerializer.Deserialize<object>(jsonReader);
            }
        }
    }
}
