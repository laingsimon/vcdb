using System.Threading.Tasks;

namespace vcdb.Output
{
    public interface IOutput
    {
        Task WriteJsonToOutput<T>(T output);
        Task WriteToOutput(string output);
    }
}