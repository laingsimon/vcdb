using System;

namespace TestFramework
{
    public class ResetCursorPosition : IDisposable
    {
        private readonly int cursorLeft;
        private readonly int cursorTop;

        public ResetCursorPosition(int? newLeft, int? newTop)
        {
            if (Console.IsOutputRedirected)
                return;

            cursorLeft = Console.CursorLeft;
            cursorTop = Console.CursorTop;

            Console.SetCursorPosition(newLeft ?? cursorLeft, newTop ?? cursorTop);
        }

        public void Dispose()
        {
            if (Console.IsOutputRedirected)
                return;

            Console.SetCursorPosition(cursorLeft, cursorTop);
        }
    }
}
