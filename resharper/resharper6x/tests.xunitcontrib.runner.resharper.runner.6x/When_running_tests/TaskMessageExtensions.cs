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
            return AssertContainsTaskMessage(taskMessages, TaskMessage.TaskStarting(expectedTask));
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskFinished(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedMessage, TaskResult expectedResult)
        {
            var expectedTaskMessage = TaskMessage.TaskFinished(expectedTask, expectedMessage, expectedResult);

            return taskMessages.AssertContainsTaskMessage(expectedTaskMessage);
        }

        public static IEnumerable<TaskMessage> AssertContainsSuccessfulTaskFinished(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask)
        {
            return taskMessages.AssertContainsTaskFinished(expectedTask, string.Empty, TaskResult.Success);
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskOutput(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedOutput)
        {
            return taskMessages.AssertContainsTaskMessage(TaskMessage.TaskOutput(expectedTask, expectedOutput, TaskOutputType.STDOUT));
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskException(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, Exception expectedException)
        {
            return taskMessages.AssertContainsTaskMessage(TaskMessage.TaskException(expectedTask, expectedException));
        }

        private static IEnumerable<TaskMessage> AssertContainsTaskMessage(this IEnumerable<TaskMessage> taskMessages, TaskMessage expectedTaskMessage)
        {
            var seen = new List<TaskMessage>();

            var x = taskMessages.ToList();

            foreach (var taskMessage in x)
            {
                seen.Add(taskMessage);
                if (taskMessage.Task == expectedTaskMessage.Task && taskMessage.Message == expectedTaskMessage.Message)
                    return new List<TaskMessage>(x.SkipWhile(tm => !(tm.Task == expectedTaskMessage.Task && tm.Message == expectedTaskMessage.Message)));
            }

            var sb = new StringBuilder();
            sb.AppendLine("Failed to find task message call.");
            sb.AppendFormat("Expected: {0} {1}", expectedTaskMessage.Task, expectedTaskMessage.Message);
            sb.AppendLine();

            sb.AppendLine("Saw:");
            foreach (var taskMessage in seen)
            {
                sb.AppendFormat("{0} {1}", taskMessage.Task, taskMessage.Message);
                sb.AppendLine();
            }
            throw new AssertException(sb.ToString());
        }
    }
}