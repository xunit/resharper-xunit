using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    partial class RemoteTaskServer
    {
        // Added in 8.0
        partial void TaskDuration(RemoteTask remoteTask, TimeSpan duration)
        {
            server.TaskDuration(remoteTask, duration);
        }
    }
}
