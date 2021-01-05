using System.Collections.Generic;
using System.Threading.Tasks;

namespace vcdb.Output
{
    public class EnumerableOutput<T> : IOutputable
    {
        private readonly IEnumerable<T> input;

        public EnumerableOutput(IEnumerable<T> input)
        {
            this.input = input;
        }

        public async Task WriteToOutput(IOutput output)
        {
            foreach (var item in input)
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
