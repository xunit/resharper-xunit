using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class TaskMessage
    {
        private TaskMessage(RemoteTask task, ServerAction serverAction, params object[] parameters)
        {
            Assert.NotNull(task);

            Task = task;
            ServerAction = serverAction;
            Message = ServerMessage.Format(serverAction, parameters);
        }

        public RemoteTask Task { get; private set; }
        public ServerAction ServerAction { get; private set; }
        public string Message { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Task, Message);
        }

        public static TaskMessage TaskDiscovered(RemoteTask task)
        {
            return new TaskMessage(task, ServerAction.TaskDiscovered);
        }

        public static TaskMessage TaskStarting(RemoteTask task)
        {
            return new TaskMessage(task, ServerAction.TaskStarting);
        }

        public static TaskMessage TaskProgress(RemoteTask task, string message)
        {
            return new TaskMessage(task, ServerAction.TaskProgress, message);
        }

        public static TaskMessage TaskError(RemoteTask task, string message)
        {
            return new TaskMessage(task, ServerAction.TaskError, message);
        }

        public static TaskMessage TaskOutput(RemoteTask task, string text, TaskOutputType outputType)
        {
            return new TaskMessage(task, ServerAction.TaskOutput, text, outputType);
        }

        public static TaskMessage TaskFinished(RemoteTask task, string message, TaskResult result)
        {
            return new TaskMessage(task, ServerAction.TaskFinished, message, result);
        }

        public static TaskMessage TaskDuration(RemoteTask task, TimeSpan duration)
        {
            return new TaskMessage(task, ServerAction.TaskDuration, duration);
        }

        public static TaskMessage TaskExplain(RemoteTask task, string explanation)
        {
            return new TaskMessage(task, ServerAction.TaskExplain, explanation);
        }

        public static TaskMessage TaskException(RemoteTask task, IEnumerable<TaskException> exceptions)
        {
            return new TaskMessage(task, ServerAction.TaskException, exceptions);
        }

        public static TaskMessage TaskException(RemoteTask task, Exception exception)
        {
            return new TaskMessage(task, ServerAction.TaskException, exception);
        }

        public static TaskMessage CreateDynamicElement(RemoteTask task)
        {
            return new TaskMessage(task, ServerAction.CreateDynamicElement);
        }
    }
}