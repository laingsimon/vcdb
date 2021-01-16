using System;

namespace TestFramework.Output
{
    public interface ILogger
    {
        IDisposable GetWriteLock();
        OutputDetail LogDebug(string message);
        OutputDetail LogError(string message);
        OutputDetail LogError(Exception exception, string message);
        OutputDetail LogInformation(string message);
        OutputDetail LogLine(string message);
        OutputDetail LogWarning(string message);
    }
}
