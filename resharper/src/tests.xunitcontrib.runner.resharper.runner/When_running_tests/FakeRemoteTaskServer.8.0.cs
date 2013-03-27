using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class FakeRemoteTaskServer : IRemoteTaskServer
    {
        private readonly IList<TaskMessage> messages = new List<TaskMessage>();

        public IEnumerable<TaskMessage> Messages
        {
            get { return messages; }
        }

        private void Add(TaskMessage message)
        {
            Console.WriteLine(message);
            messages.Add(message);
        }

        public void TaskStarting(RemoteTask remoteTask)
        {
            Add(TaskMessage.TaskStarting(remoteTask));
        }

        public void TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            Add(TaskMessage.TaskException(remoteTask, exceptions));
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
            throw new NotImplementedException();
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