using System;
using System.IO;
using System.Threading;
using TestFramework;

namespace vcdb.IntegrationTests.Framework
{
    internal class IntegrationTestLogger : ILogger
    {
        private readonly TextWriter outputWriter;
        private readonly TextWriter errorWriter;
        private readonly LogLevel minLogLevel;

        public IntegrationTestLogger(TextWriter outputWriter, TextWriter errorWriter, LogLevel minLogLevel)
        {
            this.outputWriter = outputWriter;
            this.errorWriter = errorWriter;
            this.minLogLevel = minLogLevel;
        }

        public IDisposable GetWriteLock()
        {
            return new WriteLock();
        }

        public OutputDetail LogDebug(string message)
        {
            return WriteMessage(LogLevel.Debug, message);
        }

        public OutputDetail LogError(string message)
        {
            return WriteMessage(LogLevel.Error, message);
        }

        public OutputDetail LogError(Exception exception, string message)
        {
            return WriteMessage(LogLevel.Error, $"{exception.Message}\r\n{message}");
        }

        public OutputDetail LogInformation(string message)
        {
            return WriteMessage(LogLevel.Information, message);
        }

        public OutputDetail LogLine(string message)
        {
            outputWriter.WriteLine(message);
            return new OutputDetail();
        }

        public OutputDetail LogWarning(string message)
        {
            return WriteMessage(LogLevel.Warning, message);
        }

        private OutputDetail WriteMessage(LogLevel logLevel, string message)
        {
            if (logLevel < minLogLevel)
                return null;

            var output = logLevel == LogLevel.Error
                ? errorWriter
                : outputWriter;

            using (GetWriteLock())
            {
                output.WriteLine($"{logLevel.ToString().Substring(0, 4).ToLower()}: {message}");
            }

            return new OutputDetail();
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
