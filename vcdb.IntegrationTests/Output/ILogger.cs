using System;

namespace vcdb.IntegrationTests.Output
{
    internal interface ILogger
    {
        void LogDebug(string message);
        void LogError(string message);
        void LogError(Exception exception, string message);
        void LogInformation(string message);
        void LogLine(string message);
        void LogWarning(string message);
    }
}
