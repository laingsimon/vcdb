using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace vcdb.IntegrationTests.Content
{
    internal class ReformattingJson : IJson
    {
        private readonly IJson underlying;

        public ReformattingJson(Json underlying)
        {
            this.underlying = underlying;
        }

        public JToken ReadJsonContent(string json)
        {
            return underlying.ReadJsonContent(json);
        }

        public JToken ReadJsonFromFile(string scenarioRelativePath)
        {
            var token = underlying.ReadJsonFromFile(scenarioRelativePath);
            WriteJsonContent(token, scenarioRelativePath, Formatting.Indented);
            return token;
        }

        public T ReadJsonFromFile<T>(string scenarioRelativePath)
        {
            var content = underlying.ReadJsonFromFile<T>(scenarioRelativePath);
            WriteJsonContent(content, scenarioRelativePath, Formatting.Indented);
            return content;
        }

        public void WriteJsonContent<T>(T actual, string scenarioRelativePath, Formatting formatting)
        {
            underlying.WriteJsonContent(actual, scenarioRelativePath, formatting);
        }
    }
}
