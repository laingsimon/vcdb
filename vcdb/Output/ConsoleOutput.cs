using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace vcdb.Output
{
    public class ConsoleOutput : IOutput
    {
        private readonly JsonSerializer jsonSerializer;
        private readonly TextWriter consoleOutput;

        public ConsoleOutput(JsonSerializer jsonSerializer, IOutputFactory outputFactory)
        {
            this.jsonSerializer = jsonSerializer;
            consoleOutput = outputFactory.GetActualConsoleOutput();
        }

        public async Task WriteJsonToOutput<T>(T output)
        {
            await Task.Run(() =>
            {
                using (var writer = new JsonTextWriter(consoleOutput))
                {
                    jsonSerializer.Serialize(writer, output);
                }

                consoleOutput.WriteLine();
            });
        }

        public async Task WriteToOutput(string output)
        {
            await consoleOutput.WriteLineAsync(output);
        }
    }
}
