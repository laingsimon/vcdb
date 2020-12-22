using System.Threading.Tasks;

namespace vcdb
{
    public interface IOutput
    {
        Task WriteJsonToOutput<T>(T output);
    }
}