using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class FakeRemoteTaskServer : FakeRemoteTaskServerBase, IRemoteTaskServer
    {
        public void TaskDiscovered(RemoteTask remoteTask)
        {
            Add(TaskMessage.TaskDiscovered(remoteTask));
        }

        public void TaskStarting(RemoteTask remoteTask)
        {
            Add(TaskMessage.TaskStarting(remoteTask));
        }

        public void TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            Add(TaskMessage.TaskException(remoteTask, exceptions));
            ReportExceptions(exceptions);
        }

        public void TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            Add(TaskMessage.TaskOutput(remoteTask, text, outputType));
        }

        public void TaskFinished(RemoteTask remoteTask, string message, TaskResult result)
        {
            Add(TaskMessage.TaskFinished(remoteTask, message, result));
        }

        public void TaskDuration(RemoteTask remoteTask, TimeSpan duration)
        {
            Add(TaskMessage.TaskDuration(remoteTask, duration));
        }

        public void CreateDynamicElement(RemoteTask remoteTask)
        {
            Add(TaskMessage.CreateDynamicElement(remoteTask));
        }

        public void SetTempFolderPath(string path)
        {
            throw new NotImplementedException();
        }
    }
}