using System;

namespace TestFramework.Execution
{
    public interface ITaskGate
    {
        IDisposable StartTask();
    }
}