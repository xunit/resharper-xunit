using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
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
            ShouldContinue = server.TaskStarting(remoteTask);
        }

        public void TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            ShouldContinue = server.TaskException(remoteTask, exceptions);
        }

        public void TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            ShouldContinue = server.TaskOutput(remoteTask, text, outputType);
        }

        public void TaskFinished(RemoteTask remoteTask, string message, TaskResult result)
        {
            ShouldContinue = server.TaskFinished(remoteTask, message, result);
        }

        public void TaskExplain(RemoteTask remoteTask, string explanation)
        {
            ShouldContinue = server.TaskExplain(remoteTask, explanation);
        }

        public void TaskDuration(RemoteTask remoteTask, TimeSpan duration)
        {
            // Not implemented until 8.0
        }

        public void CreateDynamicElement(RemoteTask remoteTask)
        {
            server.CreateDynamicElement(remoteTask);
        }
    }
}