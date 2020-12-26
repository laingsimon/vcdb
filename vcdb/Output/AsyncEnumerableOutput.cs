using System.Collections.Generic;
using System.Threading.Tasks;

namespace vcdb.Output
{
    public class AsyncEnumerableOutput<T> : IOutputable
    {
        private readonly IAsyncEnumerable<T> input;

        public AsyncEnumerableOutput(IAsyncEnumerable<T> input)
        {
            this.input = input;
        }

        public async Task WriteToOutput(IOutput output)
        {
            await foreach (var item in input)
            {
                await output.WriteJsonToOutput(item);
            }
        }
    }
}
