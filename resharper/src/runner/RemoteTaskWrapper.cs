using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class RemoteTaskWrapper
    {
        private readonly IRemoteTaskServer server;
        private bool started;
        private bool finished;
        private string message;
        private TaskResult result;
        private IList<RemoteTaskWrapper> children;

        public RemoteTaskWrapper(RemoteTask remoteTask, IRemoteTaskServer server)
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

        public void Failed(TaskException[] exceptions, string exceptionMessage, string childMessage = null)
        {
            if (childMessage != null)
            {
                Reset();

                var childExceptions = new[] { new TaskException(null, childMessage, null) };
                foreach (var child in children ?? EmptyList<RemoteTaskWrapper>.InstanceList)
                    child.Error(childExceptions, childMessage);
            }

            if (result == TaskResult.Inconclusive)
            {
                result = TaskResult.Exception;
                message = exceptionMessage;
                server.TaskException(RemoteTask, exceptions);
            }
        }

        public void Finished(decimal durationInSeconds = 0, bool childTestsFailed = false)
        {
            // Result is based on child tasks
            if (result == TaskResult.Inconclusive)
            {
                result = childTestsFailed ? TaskResult.Exception : TaskResult.Success;
                message = childTestsFailed ? "One or more child tests failed" : string.Empty;
            }

            // Only finish once - so a test can share a test case's task
            if (!finished)
            {
                var duration = TimeSpan.FromSeconds((double) durationInSeconds);
                if (duration >= TimeSpan.Zero)
                    server.TaskDuration(RemoteTask, duration);
                server.TaskFinished(RemoteTask, message, result);
            }
            finished = true;
        }

        private void Error(TaskException[] exceptions, string errorMessage)
        {
            Reset();

            foreach (var child in children ?? EmptyList<RemoteTaskWrapper>.InstanceList)
                child.Error(exceptions, errorMessage);

            result = TaskResult.Error;
            message = errorMessage;
            server.TaskException(RemoteTask, exceptions);

            // Error implies Finished. Also, if we're calling Error on grand-children, we need
            // to call Finished on them too, and this is the easiest way
            Finished();
        }

        private void Reset()
        {
            finished = false;
            result = TaskResult.Inconclusive;
            message = string.Empty;
        }
    }
}