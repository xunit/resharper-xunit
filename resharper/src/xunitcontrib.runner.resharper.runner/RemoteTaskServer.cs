using System;
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

            CreateClientController();
        }

        // Pre 8.0, the client controller (e.g. dotCover collected information for test coverage)
        // was created as part of setting up the AppDomain. Since we let xunit do that, no-one was
        // creating the client controller. So we do that here, and make sure it's called when
        // tasks start and finish. 8.0 no longer does AppDomain management on the runner's behalf,
        // and the client controller is created as part of the normal startup process
        partial void CreateClientController();
        partial void ReportRunStartedToClientContoller();
        partial void ReportTaskStartingToClientController(RemoteTask remoteTask);
        partial void ReportTaskFinishedToClientContoller(RemoteTask remoteFinished);
        partial void ReportAdditionInfoToClientController();
        partial void ReportRunFinishedToClientController();

        public TaskExecutorConfiguration Configuration { get; private set; }

        public void SetTempFolderPath(string path)
        {
            server.SetTempFolderPath(path);
        }

        public void TaskRunStarting()
        {
            ReportRunStartedToClientContoller();
        }

        public void TaskStarting(RemoteTask remoteTask)
        {
            ReportTaskStartingToClientController(remoteTask);
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
            ReportTaskFinishedToClientContoller(remoteTask);
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
            ReportRunFinishedToClientController();
        }

        public void CreateDynamicElement(RemoteTask remoteTask)
        {
            server.CreateDynamicElement(remoteTask);
        }
    }
}