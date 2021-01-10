using System;

namespace TestFramework
{
    internal class ResetConsoleColor : IDisposable
    {
        private readonly ConsoleColor oldForegroundColor;
        private readonly ConsoleColor oldBackgroundColor;

        public ResetConsoleColor(ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (Console.IsOutputRedirected)
                return;

            oldForegroundColor = Console.ForegroundColor;
            oldBackgroundColor = Console.BackgroundColor;

            if (foreground != null)
                Console.ForegroundColor = foreground.Value;
            if (background != null)
                Console.BackgroundColor = background.Value;
        }

        public void Dispose()
        {
            if (Console.IsOutputRedirected)
                return;

            Console.ForegroundColor = oldForegroundColor;
            Console.BackgroundColor = oldBackgroundColor;
        }
    }
}
