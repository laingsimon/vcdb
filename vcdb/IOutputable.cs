using System.Threading.Tasks;

namespace vcdb
{
    public interface IOutputable
    {
        Task WriteToOutput(IOutput output);
    }
}
