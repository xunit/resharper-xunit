using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class FakeRemoteTaskServer : FakeRemoteTaskServerBase, IRemoteTaskServer
    {
        // ReSharper 7.1
        public string GetAdditionalControllerInfo()
        {
            return string.Empty;
        }

        public bool TaskStarting(RemoteTask remoteTask)
        {
            Add(TaskMessage.TaskStarting(remoteTask));
            return true;
        }

        public bool TaskProgress(RemoteTask remoteTask, string message)
        {
            Add(TaskMessage.TaskProgress(remoteTask, message));
            return true;
        }

        public bool TaskError(RemoteTask remoteTask, string message)
        {
            Add(TaskMessage.TaskError(remoteTask, message));
            return true;
        }

        public bool TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            Add(TaskMessage.TaskException(remoteTask, exceptions));
            ReportExceptions(exceptions);
            return true;
        }

        public bool TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            Add(TaskMessage.TaskOutput(remoteTask, text, outputType));
            return true;
        }

        public bool TaskFinished(RemoteTask remoteTask, string message, TaskResult result)
        {
            Add(TaskMessage.TaskFinished(remoteTask, message, result));
            return true;
        }

        public bool TaskExplain(RemoteTask remoteTask, string explanation)
        {
            Add(TaskMessage.TaskExplain(remoteTask, explanation));
            return true;
        }

        public bool CreateDynamicElement(RemoteTask remoteTask)
        {
            Add(TaskMessage.CreateDynamicElement(remoteTask));
            return true;
        }

        public TaskRunnerClientControllerInfo ClientControllerInfo
        {
            get { return null; }
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
        #endregion
    }
}