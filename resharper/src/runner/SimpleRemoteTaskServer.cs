using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class SimpleRemoteTaskServer : ISimpleRemoteTaskServer
    {
        private readonly IRemoteTaskServer server;

        public SimpleRemoteTaskServer(IRemoteTaskServer server)
        {
            this.server = server;
        }

        public bool ShouldContinue { get; set; }

        public void SetTempFolderPath(string path)
        {
            server.SetTempFolderPath(path);
        }

        public void TaskStarting(RemoteTask remoteTask)
        {
            server.TaskStarting(remoteTask);
        }

        public void TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            server.TaskException(remoteTask, exceptions);
        }

        public void TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            server.TaskOutput(remoteTask, text, outputType);
        }

        public void TaskFinished(RemoteTask remoteTask, string message, TaskResult result)
        {
            server.TaskFinished(remoteTask, message, result);
        }

        public void TaskExplain(RemoteTask remoteTask, string explanation)
        {
            // Doesn't exist in ReSharper 8.0
        }

        public void TaskDuration(RemoteTask remoteTask, TimeSpan duration)
        {
            server.TaskDuration(remoteTask, duration);
        }

        public void CreateDynamicElement(RemoteTask remoteTask)
        {
            server.CreateDynamicElement(remoteTask);
        }

        public void ShowNotification(string message, string description)
        {
            server.ShowNotification(message, description);
        }
    }
}