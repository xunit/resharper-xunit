using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class RemoteTaskServer
    {
        private readonly IRemoteTaskServer server;
        private readonly ISimpleClientController clientController;

        public RemoteTaskServer(IRemoteTaskServer server, TaskExecutorConfiguration configuration)
        {
            this.server = server;
            Configuration = configuration;
            clientController = SimpleClientControllerFactory.Create(server);

            ShouldContinue = true;
        }

        public TaskExecutorConfiguration Configuration { get; private set; }

        public bool ShouldContinue { get; set; }

        public void SetTempFolderPath(string path)
        {
            server.SetTempFolderPath(path);
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
                TaskExplain(remoteTask, message);
            if (duration != TimeSpan.Zero)
                TaskDuration(remoteTask, duration);
            server.TaskFinished(remoteTask, message, result);
        }

        // TaskExplain no longer exists in 8.0. We only used it to pass the skip reason
        partial void TaskExplain(RemoteTask remoteTask, string explanation);
        partial void TaskDuration(RemoteTask remoteTask, TimeSpan duration);

        public void TaskRunFinished()
        {
            clientController.TaskRunFinished();
        }

        public void CreateDynamicElement(RemoteTask remoteTask)
        {
            server.CreateDynamicElement(remoteTask);
        }
    }
}