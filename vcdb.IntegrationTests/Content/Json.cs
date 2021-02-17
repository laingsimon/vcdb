using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace vcdb.IntegrationTests.Content
{
    internal class Json : IJson
    {
        private readonly JsonSerializer jsonSerializer;
        private readonly Scenario scenario;

        public Json(Scenario scenario, JsonSerializer jsonSerializer)
        {
            this.scenario = scenario;
            this.jsonSerializer = jsonSerializer;
        }

        public JToken ReadJsonFromFile(string scenarioRelativePath)
        {
            using (var reader = scenario.Read(scenarioRelativePath))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return (JToken)jsonSerializer.Deserialize<object>(jsonReader);
            }
        }

        public T ReadJsonFromFile<T>(string scenarioRelativePath)
        {
            using (var reader = scenario.Read(scenarioRelativePath))
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

        public void WriteJsonContent<T>(T actual, string scenarioRelativePath, Formatting formatting)
        {
            using (var writer = scenario.Write(scenarioRelativePath))
            using (var jsonWriter = new JsonTextWriter(writer)
            {
                Formatting = formatting
            })
            {
                jsonSerializer.Serialize(jsonWriter, actual);
            }
        }
    }
}
