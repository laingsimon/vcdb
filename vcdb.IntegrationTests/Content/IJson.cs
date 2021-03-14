using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace vcdb.IntegrationTests.Content
{
    internal interface IJson
    {
        JToken ReadJsonContent(string json);
        JToken ReadJsonFromFile(string relativePath);
        T ReadJsonFromFile<T>(string relativePath);
        void WriteJsonContent<T>(T actual, string relativePath, Formatting formatting);
    }
}
