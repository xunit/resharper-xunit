using System;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    // Legacy here means 8.2
    public class LegacySimpleRemoteTaskServer : ISimpleRemoteTaskServer
    {
        private readonly IRemoteTaskServer server;

        public LegacySimpleRemoteTaskServer(IRemoteTaskServer server)
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
            // Notify the user that something really bad has happened. It's not nice, but we'll show a message box
            MessageBox.ShowExclamation(message + Environment.NewLine + Environment.NewLine + description, "xUnit.net Test Runner");
        }
    }
}