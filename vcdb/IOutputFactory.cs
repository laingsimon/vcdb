using System.IO;

namespace vcdb
{
    public interface IOutputFactory
    {
        TextWriter GetActualConsoleOutput();
    }
}