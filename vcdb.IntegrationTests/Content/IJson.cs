using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace vcdb.IntegrationTests.Content
{
    internal interface IJson
    {
        JToken ReadJsonContent(string json);
        JToken ReadJsonFromFile(string scenarioRelativePath);
        T ReadJsonFromFile<T>(string scenarioRelativePath);
        void WriteJsonContent<T>(T actual, string scenarioRelativePath, Formatting formatting);
    }
}
