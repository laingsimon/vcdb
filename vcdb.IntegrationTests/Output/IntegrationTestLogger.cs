using System;
using System.IO;

namespace vcdb.IntegrationTests.Output
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

        public void LogDebug(string message)
        {
            WriteMessage(LogLevel.Debug, message);
        }

        public void LogError(string message)
        {
            WriteMessage(LogLevel.Error, message);
        }

        public void LogError(Exception exception, string message)
        {
            WriteMessage(LogLevel.Error, $"{exception.Message}\r\n{message}");
        }

        public void LogInformation(string message)
        {
            WriteMessage(LogLevel.Information, message);
        }

        public void LogLine(string message)
        {
            outputWriter.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            WriteMessage(LogLevel.Warning, message);
        }

        private void WriteMessage(LogLevel logLevel, string message)
        {
            if (logLevel < minLogLevel)
                return;

            var output = logLevel == LogLevel.Error
                ? errorWriter
                : outputWriter;

            output.WriteLine($"{logLevel.ToString().Substring(0, 4).ToLower()}: {message}");
        }
    }
}
