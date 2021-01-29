using System.Threading.Tasks;

namespace vcdb.Output
{
    public class OutputtableCollection : IOutputable
    {
        private readonly IOutputable[] outputtables;

        public OutputtableCollection(params IOutputable[] outputtables)
        {
            this.outputtables = outputtables;
        }

        public async Task WriteToOutput(IOutput output)
        {
            foreach (var outputable in outputtables)
            {
                await outputable.WriteToOutput(output);
            }
        }
    }
}
