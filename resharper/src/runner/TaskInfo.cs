using System;
using System.Diagnostics;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class TaskInfo
    {
        protected TaskInfo(RemoteTask remoteTask)
        {
            RemoteTask = remoteTask;
            Started = false;
            Finished = false;
            Result = TaskResult.Inconclusive;
        }

        public RemoteTask RemoteTask { get; private set; }
        public bool Started { get; set; }
        public bool Finished { get; set; }
        public TaskResult Result { get; set; }
        public string Message { get; set; }
    }

    public class ClassTaskInfo : TaskInfo
    {
        public ClassTaskInfo(XunitTestClassTask remoteTask)
            : base(remoteTask)
        {
        }

        public XunitTestClassTask ClassTask
        {
            get { return RemoteTask as XunitTestClassTask; }
        }
    }

    public class MethodTaskInfo : TaskInfo
    {
        private readonly Stopwatch stopwatch;

        public MethodTaskInfo(XunitTestMethodTask remoteTask)
            : base(remoteTask)
        {
            stopwatch = new Stopwatch();
        }

        public XunitTestMethodTask MethodTask
        {
            get { return RemoteTask as XunitTestMethodTask; }
        }

        public void Start()
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

        public TimeSpan Stop()
        {
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }

    public class TheoryTaskInfo : TaskInfo
    {
        public TheoryTaskInfo(XunitTestTheoryTask remoteTask)
            : base(remoteTask)
        {
        }

        public XunitTestTheoryTask TheoryTask
        {
            get { return RemoteTask as XunitTestTheoryTask; }
        }
    }
}