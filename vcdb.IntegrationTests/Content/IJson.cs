using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace vcdb.IntegrationTests.Content
{
    internal interface IJson
    {
        JToken ReadJsonContent(string json);
        JToken ReadJsonFromFile(params string[] relativePaths);
        T ReadJsonFromFile<T>(params string[] relativePaths);
        void WriteJsonContent<T>(T actual, string relativePath, Formatting formatting);
    }
}
