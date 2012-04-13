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
            get { return messages.Hide(); }
        }

        public bool TaskStarting(RemoteTask remoteTask)
        {
            messages.Add(TaskMessage.TaskStarting(remoteTask));
            return true;
        }

        public bool TaskProgress(RemoteTask remoteTask, string message)
        {
            messages.Add(TaskMessage.TaskProgress(remoteTask, message));
            return true;
        }

        public bool TaskError(RemoteTask remoteTask, string message)
        {
            messages.Add(TaskMessage.TaskError(remoteTask, message));
            return true;
        }

        public bool TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            messages.Add(TaskMessage.TaskException(remoteTask, exceptions));
            return true;
        }

        public bool TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            messages.Add(TaskMessage.TaskOutput(remoteTask, text, outputType));
            return true;
        }

        public bool TaskFinished(RemoteTask remoteTask, string message, TaskResult result)
        {
            messages.Add(TaskMessage.TaskFinished(remoteTask, message, result));
            return true;
        }

        public bool TaskExplain(RemoteTask remoteTask, string explanation)
        {
            messages.Add(TaskMessage.TaskExplain(remoteTask, explanation));
            return true;
        }

        #region Not implemented
        public bool Connect()
        {
            throw new NotImplementedException();
        }

        public TaskExecutorConfiguration GetConfiguration()
        {
            throw new NotImplementedException();
        }

        public void SetTempFolderPath(string path)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public RemoteTaskPacket[] GetPackets()
        {
            throw new NotImplementedException();
        }

        public RemoteTaskRunnerInfo GetTaskRunnerInfo(string taskRunnerID)
        {
            throw new NotImplementedException();
        }

        public bool ClientMessage(string message)
        {
            throw new NotImplementedException();
        }

        public IRemoteTaskServer WithoutProxy
        {
            get { throw new NotImplementedException(); }
        }

        public TaskRunnerClientControllerInfo ClientControllerInfo
        {
            get { throw new NotImplementedException(); }
        }

        public bool CreateDynamicElement(RemoteTask remoteTask)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}