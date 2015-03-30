using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class RemoteTaskWrapper
    {
        private readonly RemoteTaskServer server;
        private bool started;
        private bool finished;
        private string message;
        private TaskResult result;
        private IList<RemoteTaskWrapper> children;

        public RemoteTaskWrapper(RemoteTask remoteTask, RemoteTaskServer server)
        {
            RemoteTask = remoteTask;
            this.server = server;

            result = TaskResult.Inconclusive;
        }

        public void AddChild(RemoteTaskWrapper child)
        {
            if (children == null)
                children = new List<RemoteTaskWrapper>();
            children.Add(child);
        }

        public RemoteTask RemoteTask { get; private set; }

        public void Starting()
        {
            // Only start once - so a test can share a test case's task
            if (!started)
                server.TaskStarting(RemoteTask);
            started = true;
        }

        public void Output(string output)
        {
            if (!string.IsNullOrEmpty(output))
                server.TaskOutput(RemoteTask, output, TaskOutputType.STDOUT);
        }

        public void Skipped(string reason)
        {
            if (result == TaskResult.Inconclusive)
            {
                result = TaskResult.Skipped;
                message = reason;
            }
        }

        public void Passed()
        {
            if (result == TaskResult.Inconclusive)
                result = TaskResult.Success;
        }

        public void Failed(TaskException[] exceptions, string exceptionMessage)
        {
            if (result == TaskResult.Inconclusive)
            {
                result = TaskResult.Exception;
                message = exceptionMessage;
                server.TaskException(RemoteTask, exceptions);
            }
        }

        public void ForceFailed(TaskException[] exceptions, string exceptionMessage)
        {
            result = TaskResult.Exception;
            message = exceptionMessage;
            server.TaskException(RemoteTask, exceptions);

            // We're forcing a failure, make sure we finish again.
            finished = false;
        }

        public void Error(TaskException[] exceptions, string errorMessage)
        {
            result = TaskResult.Error;
            message = errorMessage;
            server.TaskException(RemoteTask, exceptions);

            // Error overrides anything we've already set, so we can finish twice
            finished = false;
        }

        public void Finished(decimal durationInSeconds = 0, bool childTestsFailed = false)
        {
            if (result == TaskResult.Inconclusive)
            {
                result = childTestsFailed ? TaskResult.Exception : TaskResult.Success;
                message = childTestsFailed ? "One or more child tests failed" : string.Empty;
            }

            // Only finish once - so a test can share a test case's task
            if (!finished)
                server.TaskFinished(RemoteTask, message, result, durationInSeconds);
            finished = true;
        }
    }
}