using Newtonsoft.Json.Linq;

namespace TestFramework
{
    public interface IJson
    {
        JToken ReadJsonContent(string json);
        JToken ReadJsonFromFile(string scenarioRelativePath);
        T ReadJsonFromFile<T>(string scenarioRelativePath);
    }
}
