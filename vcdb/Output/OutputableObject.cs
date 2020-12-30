using System.Threading.Tasks;

namespace vcdb.Output
{
    internal class OutputableObject<T> : IOutputable
    {
        private T value;

        public OutputableObject(T value)
        {
            this.value = value;
        }

        public async Task WriteToOutput(IOutput output)
        {
            await output.WriteJsonToOutput(value);
        }
    }
}