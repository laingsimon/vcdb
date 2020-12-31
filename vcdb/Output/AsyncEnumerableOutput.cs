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
                if (item == null)
                    continue;

                var outputtable = item as IOutputable;

                if (outputtable != null)
                    await outputtable.WriteToOutput(output);
                else
                    await output.WriteJsonToOutput(item);
            }
        }
    }
}
