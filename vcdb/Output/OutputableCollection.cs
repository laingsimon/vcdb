using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vcdb.Output
{
    public class OutputableCollection : IOutputable
    {
        private readonly IOutputable[] outputables;

        public OutputableCollection(params IOutputable[] outputables)
        {
            this.outputables = outputables;
        }

        public OutputableCollection(IEnumerable<IOutputable> outputables)
        {
            this.outputables = outputables.ToArray();
        }

        public async Task WriteToOutput(IOutput output)
        {
            foreach (var outputable in outputables)
            {
                await outputable.WriteToOutput(output);
            }
        }
    }
}
