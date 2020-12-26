using System.IO;

namespace vcdb.Output
{
    public interface IOutputFactory
    {
        TextWriter GetActualConsoleOutput();
    }
}