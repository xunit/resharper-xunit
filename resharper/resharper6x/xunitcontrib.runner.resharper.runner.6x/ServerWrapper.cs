using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class ServerWrapper : IRemoteTaskServer
    {
        private readonly IRemoteTaskServer server;
        private readonly ITaskRunnerClientController clientController;

        public ServerWrapper(IRemoteTaskServer server, ITaskRunnerClientController clientController)
        {
            this.server = server;
            this.clientController = clientController;
        }

        public bool Connect()
        {
            return server.Connect();
        }

        public TaskExecutorConfiguration GetConfiguration()
        {
            return server.GetConfiguration();
        }

        public void SetTempFolderPath(string path)
        {
            server.SetTempFolderPath(path);
        }

        public void Disconnect()
        {
            server.Disconnect();
        }

        public RemoteTaskPacket[] GetPackets()
        {
            return server.GetPackets();
        }

        public RemoteTaskRunnerInfo GetTaskRunnerInfo(string taskRunnerID)
        {
            return server.GetTaskRunnerInfo(taskRunnerID);
        }

        public bool ClientMessage(string message)
        {
            return server.ClientMessage(message);
        }

        public bool TaskStarting(RemoteTask remoteTask)
        {
            var message = clientController.BeforeTaskStarted(remoteTask);
            if (!string.IsNullOrEmpty(message))
                server.ClientMessage(message);

            return server.TaskStarting(remoteTask);
        }

        public bool TaskProgress(RemoteTask remoteTask, string message)
        {
            return server.TaskProgress(remoteTask, message);
        }

        public bool TaskError(RemoteTask remoteTask, string message)
        {
            return server.TaskError(remoteTask, message);
        }

        public bool TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            return server.TaskException(remoteTask, exceptions);
        }

        public bool TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            return server.TaskOutput(remoteTask, text, outputType);
        }

        public bool TaskFinished(RemoteTask remoteTask, string message, TaskResult result)
        {
            var clientMessage = clientController.AfterTaskFinished(remoteTask);
            if (!string.IsNullOrEmpty(clientMessage))
                server.ClientMessage(clientMessage);

            return server.TaskFinished(remoteTask, message, result);
        }

        public bool TaskExplain(RemoteTask remoteTask, string explanation)
        {
            return server.TaskExplain(remoteTask, explanation);
        }

        public bool CreateDynamicElement(RemoteTask remoteTask)
        {
            return server.CreateDynamicElement(remoteTask);
        }

        public TaskRunnerClientControllerInfo ClientControllerInfo
        {
            get { return server.ClientControllerInfo; }
        }

        public IRemoteTaskServer WithoutProxy
        {
            get { return server.WithoutProxy; }
        }
    }
}