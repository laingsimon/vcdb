using System;
using System.IO;
using System.Threading;

namespace TestFramework
{
    public class ConsoleLogger : ILogger
    {
        private readonly int minLogLevel;

        public ConsoleLogger(Options options)
        {
            minLogLevel = (int)options.MinLogLevel;
        }

        public OutputDetail LogInformation(string message)
        {
            return LogMessage(LogLevel.Information, message);
        }

        public OutputDetail LogWarning(string message)
        {
            return LogMessage(LogLevel.Warning, message);
        }

        public OutputDetail LogError(string message)
        {
            return LogMessage(LogLevel.Error, message);
        }

        public OutputDetail LogError(Exception exception, string message)
        {
            return LogMessage(LogLevel.Error, $"{message}{Environment.NewLine}{exception}");
        }

        public OutputDetail LogDebug(string message)
        {
            return LogMessage(LogLevel.Debug, message);
        }

        public OutputDetail LogLine(string message)
        {
            using (GetWriteLock())
            {
                Console.Out.WriteLine(message);
                return new OutputDetail
                {
                    EndingConsoleTop = GetConsoleTop(Console.Out),
                    EndingConsoleLeft = message.Length
                };
            }
        }

        private OutputDetail LogMessage(LogLevel logLevel, string message)
        {
            if ((int)logLevel < minLogLevel)
                return null;

            var output = logLevel == LogLevel.Error
                ? Console.Error
                : Console.Out;

            var prefix = logLevel.ToString().Substring(0, 4).ToLower();
            var outputMessage = $"{prefix}: {message}";
            var foregroundColor = GetForegroundColor(logLevel);

            using (GetWriteLock())
            {
                using (new ResetConsoleColor(foreground: foregroundColor))
                {
                    output.Write(prefix + ": ");
                }

                output.WriteLine(message);
                return new OutputDetail
                {
                    EndingConsoleTop = GetConsoleTop(output),
                    EndingConsoleLeft = outputMessage.Length
                };
            }
        }

        private int? GetConsoleTop(TextWriter output)
        {
            if (ReferenceEquals(output, Console.Out) && !Console.IsOutputRedirected)
            {
                return Console.CursorTop;
            }

            if (ReferenceEquals(output, Console.Error) && !Console.IsErrorRedirected)
            {
                return Console.CursorTop;
            }

            return null;
        }

        private int? GetConsoleLeft(TextWriter output)
        {
            if (ReferenceEquals(output, Console.Out) && !Console.IsOutputRedirected)
            {
                return Console.CursorLeft;
            }

            if (ReferenceEquals(output, Console.Error) && !Console.IsErrorRedirected)
            {
                return Console.CursorLeft;
            }

            return null;
        }

        private ConsoleColor GetForegroundColor(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Information:
                    return ConsoleColor.DarkGreen;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Debug:
                    return ConsoleColor.DarkYellow;
                default:
                    return ConsoleColor.Blue;
            }
        }

        public IDisposable GetWriteLock()
        {
            return new WriteLock();
        }

        private class WriteLock : IDisposable
        {
            private static readonly object lockObject = new object();
            private readonly bool lockWasAcquired;

            public WriteLock()
            {
                Monitor.Enter(lockObject, ref lockWasAcquired);
            }

            public void Dispose()
            {
                if (lockWasAcquired)
                    Monitor.Exit(lockObject);
            }
        }
    }
}
