using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestFramework.Content
{
    public interface IJson
    {
        JToken ReadJsonContent(string json);
        JToken ReadJsonFromFile(string scenarioRelativePath);
        T ReadJsonFromFile<T>(string scenarioRelativePath);
        void WriteJsonContent(JToken actual, string fullPath, Formatting formatting);
    }
}
