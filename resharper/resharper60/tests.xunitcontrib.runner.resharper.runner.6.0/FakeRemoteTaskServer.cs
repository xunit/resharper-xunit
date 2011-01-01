using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests
{
    public class FakeRemoteTaskServer : IRemoteTaskServer
    {
        #region Not needed
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

        public RemoteTaskRunnerInfo GetTaskRunnerInfo(string taskRunnerId)
        {
            throw new NotImplementedException();
        }

        public bool ClientMessage(string message)
        {
            throw new NotImplementedException();
        }

        public IList<RemoteTaskRunnerInfo> GetRunnerInfos()
        {
            throw new NotImplementedException();
        }

        public bool CreateDynamicElement(RemoteTask remoteTask)
        {
            throw new NotImplementedException();
        }

        public TaskRunnerClientControllerInfo ClientControllerInfo
        {
            get { throw new NotImplementedException(); }
        }

        public IRemoteTaskServer WithoutProxy
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        readonly Stack<RemoteTask> tasks = new Stack<RemoteTask>();

        public readonly List<RemoteTask> TaskStartingCalls = new List<RemoteTask>();

        public bool TaskStarting(RemoteTask remoteTask)
        {
            Assert.NotNull(remoteTask);

            tasks.Push(remoteTask);
            TaskStartingCalls.Add(remoteTask);
            return true;
        }

        public readonly Dictionary<RemoteTask, List<string>> TaskProgressCalls = new Dictionary<RemoteTask, List<string>>();

        public bool TaskProgress(RemoteTask remoteTask, string message)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Peek(), remoteTask);

            if (!TaskProgressCalls.ContainsKey(remoteTask))
                TaskProgressCalls.Add(remoteTask, new List<string>());
            TaskProgressCalls[remoteTask].Add(message);
            return true;
        }

        public bool TaskError(RemoteTask remoteTask, string message)
        {
            throw new NotImplementedException();
        }

        public readonly Dictionary<RemoteTask, List<TaskException[]>> TaskExceptionCalls = new Dictionary<RemoteTask, List<TaskException[]>>();

        public bool TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Peek(), remoteTask);

            if (!TaskExceptionCalls.ContainsKey(remoteTask))
                TaskExceptionCalls.Add(remoteTask, new List<TaskException[]>());
            TaskExceptionCalls[remoteTask].Add(exceptions);
            return true;
        }

        public readonly Dictionary<RemoteTask, string> TaskOutputCalls = new Dictionary<RemoteTask, string>();

        public bool TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Peek(), remoteTask);
            Assert.Equal(TaskOutputType.STDOUT, outputType);

            if(!TaskOutputCalls.ContainsKey(remoteTask))
                TaskOutputCalls.Add(remoteTask, string.Empty);
            TaskOutputCalls[remoteTask] += text;
            return true;
        }

        public class TaskFinishedParameters
        {
            public RemoteTask RemoteTask;
            public string Message;
            public TaskResult Result;

            public TaskFinishedParameters(RemoteTask remoteTask, string message, TaskResult result)
            {
                RemoteTask = remoteTask;
                Message = message;
                Result = result;
            }
        }

        public readonly List<TaskFinishedParameters> TaskFinishedCalls = new List<TaskFinishedParameters>();

        public bool TaskFinished(RemoteTask remoteTask, string message, TaskResult result)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Pop(), remoteTask);
            TaskFinishedCalls.Add(new TaskFinishedParameters(remoteTask, message, result));

            return true;
        }

        public readonly Dictionary<RemoteTask, List<string>> TaskExplainCalls = new Dictionary<RemoteTask, List<string>>();

        public bool TaskExplain(RemoteTask remoteTask, string explanation)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Peek(), remoteTask);

            if (!TaskExplainCalls.ContainsKey(remoteTask))
                TaskExplainCalls.Add(remoteTask, new List<string>());
            TaskExplainCalls[remoteTask].Add(explanation);

            return true;
        }
    }
}