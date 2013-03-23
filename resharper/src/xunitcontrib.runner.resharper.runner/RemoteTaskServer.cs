using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class RemoteTaskServer
    {
        private readonly IRemoteTaskServer server;

        public RemoteTaskServer(IRemoteTaskServer server, TaskExecutorConfiguration configuration)
        {
            Configuration = configuration;
            this.server = server;
        }

        public TaskExecutorConfiguration Configuration { get; private set; }

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
            if (result == TaskResult.Skipped)
                TaskExplain(remoteTask, message);
            server.TaskFinished(remoteTask, message, result);
        }

        partial void TaskExplain(RemoteTask remoteTask, string explanation);

        public void CreateDynamicElement(RemoteTask remoteTask)
        {
            server.CreateDynamicElement(remoteTask);
        }
    }
}