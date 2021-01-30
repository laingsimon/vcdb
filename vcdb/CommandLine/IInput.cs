using System.IO;
using System.Threading.Tasks;

namespace vcdb.CommandLine
{
    public interface IInput
    {
        Task<T> Read<T>();
        string GetHash(int hashSize);
        TextReader GetSiblingContent(string fileName);
    }
}