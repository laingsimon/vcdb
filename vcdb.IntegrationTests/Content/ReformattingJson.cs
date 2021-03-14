using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace vcdb.IntegrationTests.Content
{
    internal class ReformattingJson : IJson
    {
        private readonly Json underlying;

        public ReformattingJson(Scenario scenario, JsonSerializer jsonSerializer)
        {
            underlying = new Json(scenario, jsonSerializer);
        }

        public JToken ReadJsonContent(string json)
        {
            return underlying.ReadJsonContent(json);
        }

        public JToken ReadJsonFromFile(string relativePath)
        {
            var result = underlying.ReadJsonFromFile(relativePath);
            if (!string.IsNullOrEmpty(result.Path))
            {
                WriteJsonContent(result.Content, Path.GetFileName(result.Path), Formatting.Indented);
            }
            return result.Content;
        }

        public T ReadJsonFromFile<T>(string relativePath)
        {
            var result = underlying.ReadJsonFromFile<T>(relativePath);
            if (!string.IsNullOrEmpty(result.Path))
            {
                WriteJsonContent(result.Content, Path.GetFileName(result.Path), Formatting.Indented);
            }
            return result.Content;
        }

        public void WriteJsonContent<T>(T actual, string relativePath, Formatting formatting)
        {
            underlying.WriteJsonContent(actual, relativePath, formatting);
        }

        private class Json
        {
            private readonly JsonSerializer jsonSerializer;
            private readonly Scenario scenario;

            public Json(Scenario scenario, JsonSerializer jsonSerializer)
            {
                this.scenario = scenario;
                this.jsonSerializer = jsonSerializer;
            }

            public FileContent<JToken> ReadJsonFromFile(string relativePath)
            {
                using (var reader = scenario.Read(relativePath))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return new FileContent<JToken>(
                        (JToken)jsonSerializer.Deserialize<object>(jsonReader),
                        reader);
                }
            }

            public FileContent<T> ReadJsonFromFile<T>(string relativePath)
            {
                using (var reader = scenario.Read(relativePath))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return new FileContent<T>(
                        jsonSerializer.Deserialize<T>(jsonReader),
                        reader);
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

        private class FileContent<T>
        {
            public FileContent(T content, TextReader reader)
            {
                Content = content;
                Path = GetFileName(reader);
            }

            public T Content { get; }
            public string Path { get; }

            private static string GetFileName(TextReader reader)
            {
                var streamReader = reader as StreamReader;
                var fileStream = streamReader?.BaseStream as FileStream;

                return fileStream?.Name;
            }
        }
    }
}
