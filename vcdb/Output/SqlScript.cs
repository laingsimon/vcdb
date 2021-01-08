using System.Diagnostics;
using System.Threading.Tasks;

namespace vcdb.Output
{
    [DebuggerDisplay("{content,nq}")]
    public class SqlScript : IOutputable
    {
        private readonly string content;

        public SqlScript(string content)
        {
            this.content = content;
        }

        public async Task WriteToOutput(IOutput output)
        {
            await output.WriteToOutput(content);
        }
    }
}
