using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class RemoteTaskServer
    {
        private readonly ISimpleRemoteTaskServer server;
        private readonly ISimpleClientController clientController;
        private readonly ISetTempFolderPathStrategy setTempFolderPathStrategy;

        public RemoteTaskServer(IRemoteTaskServer server, TaskExecutorConfiguration configuration)
        {
            this.server = SimpleRemoteTaskServerFactory.Create(server);
            Configuration = configuration;
            clientController = SimpleClientControllerFactory.Create(server);
            setTempFolderPathStrategy = SetTempFolderPathStrategyFactory.Create(server);

            ShouldContinue = true;
        }

        public TaskExecutorConfiguration Configuration { get; private set; }

        public bool ShouldContinue
        {
            get { return server.ShouldContinue; }
            set { server.ShouldContinue = value; }
        }

        public void SetTempFolderPath(string path)
        {
            setTempFolderPathStrategy.SetTempFolderPath(path);
        }

        public void TaskRunStarting()
        {
            clientController.TaskRunStarting();
        }

        public void TaskStarting(RemoteTask remoteTask)
        {
            clientController.TaskStarting(remoteTask);
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
            TaskFinished(remoteTask, message, result, TimeSpan.Zero);
        }

        public void TaskFinished(RemoteTask remoteTask, string message, TaskResult result, TimeSpan duration)
        {
            clientController.TaskFinished(remoteTask);
            if (result == TaskResult.Skipped)
                server.TaskExplain(remoteTask, message);
            if (duration != TimeSpan.Zero)
                server.TaskDuration(remoteTask, duration);
            server.TaskFinished(remoteTask, message, result);
        }

        public void TaskRunFinished()
        {
            clientController.TaskRunFinished();
            setTempFolderPathStrategy.TestRunFinished();
        }

        public void CreateDynamicElement(RemoteTask remoteTask)
        {
            server.CreateDynamicElement(remoteTask);
        }
    }
}