using System.Threading.Tasks;

namespace vcdb.Output
{
    public interface IOutputable
    {
        Task WriteToOutput(IOutput output);
    }
}
