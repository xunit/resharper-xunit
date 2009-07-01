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

        public IList<RemoteTaskRunnerInfo> GetRunnerInfos()
        {
            throw new NotImplementedException();
        }

        public IRemoteTaskServer WithoutProxy
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        Stack<RemoteTask> tasks = new Stack<RemoteTask>();

        public readonly List<RemoteTask> TaskStartingCalls = new List<RemoteTask>();

        public void TaskStarting(RemoteTask remoteTask)
        {
            Assert.NotNull(remoteTask);

            tasks.Push(remoteTask);
            TaskStartingCalls.Add(remoteTask);
        }

        public readonly Dictionary<RemoteTask, List<string>> TaskProgressCalls = new Dictionary<RemoteTask, List<string>>();

        public void TaskProgress(RemoteTask remoteTask, string message)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Peek(), remoteTask);

            if (!TaskProgressCalls.ContainsKey(remoteTask))
                TaskProgressCalls.Add(remoteTask, new List<string>());
            TaskProgressCalls[remoteTask].Add(message);
        }

        public void TaskError(RemoteTask remoteTask, string message)
        {
            throw new NotImplementedException();
        }

        public readonly Dictionary<RemoteTask, List<TaskException[]>> TaskExceptionCalls = new Dictionary<RemoteTask, List<TaskException[]>>();

        public void TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Peek(), remoteTask);

            if (!TaskExceptionCalls.ContainsKey(remoteTask))
                TaskExceptionCalls.Add(remoteTask, new List<TaskException[]>());
            TaskExceptionCalls[remoteTask].Add(exceptions);
        }

        public readonly Dictionary<RemoteTask, string> TaskOutputCalls = new Dictionary<RemoteTask, string>();

        public void TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Peek(), remoteTask);
            Assert.Equal(TaskOutputType.STDOUT, outputType);

            if(!TaskOutputCalls.ContainsKey(remoteTask))
                TaskOutputCalls.Add(remoteTask, string.Empty);
            TaskOutputCalls[remoteTask] += text;
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

        public void TaskFinished(RemoteTask remoteTask, string message, TaskResult result)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Pop(), remoteTask);
            TaskFinishedCalls.Add(new TaskFinishedParameters(remoteTask, message, result));
        }

        public readonly Dictionary<RemoteTask, List<string>> TaskExplainCalls = new Dictionary<RemoteTask, List<string>>();

        public void TaskExplain(RemoteTask remoteTask, string explanation)
        {
            Assert.NotNull(remoteTask);
            Assert.Same(tasks.Peek(), remoteTask);

            if (!TaskExplainCalls.ContainsKey(remoteTask))
                TaskExplainCalls.Add(remoteTask, new List<string>());
            TaskExplainCalls[remoteTask].Add(explanation);
        }
    }
}