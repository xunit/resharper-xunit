using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class TaskMessage
    {
        public TaskMessage(RemoteTask task, string message)
        {
            Task = task;
            Message = message;
        }

        public RemoteTask Task { get; private set; }
        public string Message { get; private set; }

        public static TaskMessage TaskStarting(RemoteTask task)
        {
            return new TaskMessage(task, "TaskStarting");
        }

        public static TaskMessage TaskProgress(RemoteTask task, string message)
        {
            return new TaskMessage(task, string.Format("TaskProgress: {0}", message));
        }

        public static TaskMessage TaskError(RemoteTask task, string message)
        {
            return new TaskMessage(task, string.Format("TaskError: {0}", message));
        }

        public static TaskMessage TaskOutput(RemoteTask task, string text, TaskOutputType outputType)
        {
            return new TaskMessage(task, string.Format("TaskOutput: {0} - <{1}>", outputType, text));
        }

        public static TaskMessage TaskFinished(RemoteTask task, string message, TaskResult result)
        {
            return new TaskMessage(task, string.Format("TaskFinished: {0} - <{1}>", result, message));
        }

        public static TaskMessage TaskExplain(RemoteTask task, string explanation)
        {
            return new TaskMessage(task, string.Format("TaskExplain: {0}", explanation));
        }
    }
}