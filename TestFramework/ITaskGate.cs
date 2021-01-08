using System;

namespace TestFramework
{
    public interface ITaskGate
    {
        IDisposable StartTask();
    }
}