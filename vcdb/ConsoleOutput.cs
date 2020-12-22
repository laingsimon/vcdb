using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace vcdb
{
    public class ConsoleOutput : IOutput
    {
        private readonly JsonSerializer jsonSerializer;
        private readonly TextWriter consoleOutput;

        public ConsoleOutput(JsonSerializer jsonSerializer, IOutputFactory outputFactory)
        {
            this.jsonSerializer = jsonSerializer;
            this.consoleOutput = outputFactory.GetActualConsoleOutput();
        }

        public async Task WriteJsonToOutput<T>(T output)
        {
            await Task.Run(() =>
            {
                using (var writer = new JsonTextWriter(consoleOutput))
                {
                    jsonSerializer.Serialize(writer, output);
                }
            });
        }
    }
}
