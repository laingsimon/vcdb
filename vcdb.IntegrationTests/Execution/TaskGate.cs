﻿using System;
using System.Threading;

namespace vcdb.IntegrationTests.Execution
{
    internal class TaskGate
    {
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private readonly int? maxConcurrency;
        private int runningTasks;

        public TaskGate(IntegrationTestOptions options)
        {
            maxConcurrency = options.MaxConcurrency;
        }

        public IDisposable StartTask()
        {
            if (maxConcurrency != null && runningTasks >= maxConcurrency.Value)
            {
                autoResetEvent.WaitOne();
            }

            runningTasks++;
            return new EndTask(this);
        }

        private void TaskHasFinished()
        {
            runningTasks--;
            autoResetEvent.Set();
        }

        private class EndTask : IDisposable
        {
            private readonly TaskGate taskGate;

            public EndTask(TaskGate taskGate)
            {
                this.taskGate = taskGate;
            }

            public void Dispose()
            {
                taskGate.TaskHasFinished();
            }
        }
    }
}