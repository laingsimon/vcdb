using System;
using System.IO;

namespace vcdb.Output
{
    internal class OutputFactory : IOutputFactory
    {
        private TextWriter consoleOut;

        public void SetActualConsoleOutput(TextWriter consoleOut)
        {
            this.consoleOut = consoleOut;
        }

        public TextWriter GetActualConsoleOutput()
        {
            return consoleOut ?? Console.Out;
        }
    }
}
