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
            // Any non-xunit exceptions that cause a test method to fail are displayed with full type name
            return taskMessages.AssertContainsTaskFinished(expectedTask, exception.GetType().FullName + ": " + exception.Message, TaskResult.Exception);
        }

        public static IEnumerable<TaskMessage> AssertContainsFailedInfrastructureTaskFinished(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, Exception exception)
        {
            // All "infrastructure" exceptions thrown when a class fails are displayed with short type name.
            // This is how the nunit runner works, because it's using a JetBrains method that only uses the
            // short type, and it also uses an nunit method that passes in the full type name. We're in a very
            // similar situation
            return taskMessages.AssertContainsTaskFinished(expectedTask, exception.GetType().Name + ": " + exception.Message, TaskResult.Exception);
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskOutput(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedOutput)
        {
            return taskMessages.AssertContains(TaskMessage.TaskOutput(expectedTask, expectedOutput, TaskOutputType.STDOUT));
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskException(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, Exception expectedException)
        {
            return taskMessages.AssertContains(TaskMessage.TaskException(expectedTask, expectedException));
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskExceptionMessage(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, Exception expectedException)
        {
            var taskMessage = TaskMessage.TaskException(expectedTask, expectedException);
            return taskMessages.AssertContains(taskMessage, m => m.StartsWith(taskMessage.Message));
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskExplain(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedExplanation)
        {
            return taskMessages.AssertContains(TaskMessage.TaskExplain(expectedTask, expectedExplanation));
        }

        public static IEnumerable<TaskMessage> AssertContains(this IEnumerable<TaskMessage> taskMessages, TaskMessage expectedTaskMessage)
        {
            return taskMessages.AssertContains(expectedTaskMessage, m => m == expectedTaskMessage.Message);
        }

        private static IEnumerable<TaskMessage> AssertContains(this IEnumerable<TaskMessage> taskMessages, TaskMessage expectedTaskMessage, Predicate<string> messageChecker) 
        {
            taskMessages = taskMessages.ToList();

            if (taskMessages.Any(tm => tm.Task.Equals(expectedTaskMessage.Task) && messageChecker(tm.Message)))
                return new List<TaskMessage>(taskMessages.SkipWhile(tm => !(tm.Task.Equals(expectedTaskMessage.Task) && messageChecker(tm.Message))).Skip(1));

            var sb = new StringBuilder();
            sb.AppendLine("Failed to find task message call.");
            sb.AppendFormat("Expected: {0}", expectedTaskMessage);
            throw new AssertException(sb.ToString());
        }

        public static void AssertDoesNotContain(this IEnumerable<TaskMessage> taskMessages, TaskMessage expectedTaskMessage)
        {
            if (!taskMessages.Any(tm => tm.Task.Equals(expectedTaskMessage.Task) && tm.Message == expectedTaskMessage.Message))
                return;

            throw new AssertException(string.Format("Was not expecting to find: {0}", expectedTaskMessage));
        }
    }
}