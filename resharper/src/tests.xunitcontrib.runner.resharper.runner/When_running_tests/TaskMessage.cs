using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class TaskMessage
    {
        private TaskMessage(RemoteTask task, string message)
        {
            Assert.NotNull(task);

            Task = task;
            Message = message;
        }

        public RemoteTask Task { get; private set; }
        public string Message { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Task, Message);
        }

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

        public static TaskMessage TaskException(RemoteTask task, IEnumerable<TaskException> exceptions)
        {
            var formattedExceptions = from exception in exceptions
                                      select FormatException(exception.Type, exception.Message, exception.StackTrace);
            var exceptionText = string.Join(Environment.NewLine, formattedExceptions.ToArray());
            return TaskException(task, exceptionText);
        }

        public static TaskMessage TaskException(RemoteTask task, Exception exception)
        {
            var exceptions = new List<string>();
            while(exception != null)
            {
                exceptions.Add(FormatException(exception.GetType().FullName, exception.Message, exception.StackTrace));
                exception = exception.InnerException;
            }

            var exceptionText = string.Join(Environment.NewLine, exceptions.ToArray());

            return TaskException(task, exceptionText);
        }

        private static string FormatException(string type, string message, string stackTrace)
        {
            return string.Format("  -> {0}: {1} {2}", type, message, stackTrace);
        }

        private static TaskMessage TaskException(RemoteTask task, string exceptionText)
        {
            return new TaskMessage(task, string.Format("TaskException:{0}{1}", Environment.NewLine, exceptionText));
        }

        public static TaskMessage CreateDynamicElement(RemoteTask task)
        {
            return new TaskMessage(task, ServerMessage.CreateDynamicElement());
        }
    }
}