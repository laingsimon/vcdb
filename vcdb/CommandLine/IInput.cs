using System.Threading.Tasks;

namespace vcdb.CommandLine
{
    public interface IInput
    {
        Task<T> Read<T>();
    }
}