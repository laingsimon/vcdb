using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace vcdb.CommandLine
{
    public class Input : IInput
    {
        private readonly Options options;
        private readonly JsonSerializer jsonSerializer;

        public Input(Options options, JsonSerializer jsonSerializer)
        {
            this.options = options;
            this.jsonSerializer = jsonSerializer;
        }

        public async Task<T> Read<T>()
        {
            if (string.IsNullOrEmpty(options.InputFile))
            {
                if (Console.IsInputRedirected)
                    return await ReadFromStream<T>(Console.In);

                return default;
            }

            return await ReadFromStream<T>(new StreamReader(options.InputFile));
        }

        private Task<T> ReadFromStream<T>(TextReader inputStreamReader)
        {
            return Task.Run(() =>
            {
                using (var jsonReader = new JsonTextReader(inputStreamReader))
                    return jsonSerializer.Deserialize<T>(jsonReader);
            });
        }
    }
}
