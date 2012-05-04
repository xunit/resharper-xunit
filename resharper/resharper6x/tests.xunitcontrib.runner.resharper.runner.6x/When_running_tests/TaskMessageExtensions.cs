using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;
using System.Linq;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class TaskMessageExtensions
    {
        public static IEnumerable<TaskMessage> AssertContainsTaskStarting(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask)
        {
            return AssertContains(taskMessages, TaskMessage.TaskStarting(expectedTask));
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskFinished(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedMessage, TaskResult expectedResult)
        {
            var expectedTaskMessage = TaskMessage.TaskFinished(expectedTask, expectedMessage, expectedResult);

            return taskMessages.AssertContains(expectedTaskMessage);
        }

        public static IEnumerable<TaskMessage> AssertContainsSuccessfulTaskFinished(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask)
        {
            return taskMessages.AssertContainsTaskFinished(expectedTask, string.Empty, TaskResult.Success);
        }

        public static IEnumerable<TaskMessage> AssertContainsFailedTaskFinished(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, Exception exception)
        {
            return taskMessages.AssertContainsTaskFinished(expectedTask, exception.GetType().FullName + ": " + exception.Message, TaskResult.Exception);
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskOutput(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedOutput)
        {
            return taskMessages.AssertContains(TaskMessage.TaskOutput(expectedTask, expectedOutput, TaskOutputType.STDOUT));
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskException(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, Exception expectedException)
        {
            return taskMessages.AssertContains(TaskMessage.TaskException(expectedTask, expectedException));
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskExplain(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedExplanation)
        {
            return taskMessages.AssertContains(TaskMessage.TaskExplain(expectedTask, expectedExplanation));
        }

        public static IEnumerable<TaskMessage> AssertContains(this IEnumerable<TaskMessage> taskMessages, TaskMessage expectedTaskMessage)
        {
            taskMessages = taskMessages.ToList();

            if (taskMessages.Any(tm => tm.Task == expectedTaskMessage.Task && tm.Message == expectedTaskMessage.Message))
                return new List<TaskMessage>(taskMessages.SkipWhile(tm => !(tm.Task == expectedTaskMessage.Task && tm.Message == expectedTaskMessage.Message)).Skip(1));

            var sb = new StringBuilder();
            sb.AppendLine("Failed to find task message call.");
            sb.AppendFormat("Expected: {0} {1}", expectedTaskMessage.Task, expectedTaskMessage.Message);
            sb.AppendLine();

            sb.AppendLine("Saw:");
            foreach (var taskMessage in taskMessages)
            {
                sb.AppendFormat("{0} {1}", taskMessage.Task, taskMessage.Message);
                sb.AppendLine();
            }
            throw new AssertException(sb.ToString());
        }

        public static void AssertDoesNotContain(this IEnumerable<TaskMessage> taskMessages, TaskMessage expectedTaskMessage)
        {
            if (!taskMessages.Any(tm => tm.Task == expectedTaskMessage.Task && tm.Message == expectedTaskMessage.Message))
                return;

            throw new AssertException(string.Format("Was not expecting to find: {0}", expectedTaskMessage));
        }
    }
}