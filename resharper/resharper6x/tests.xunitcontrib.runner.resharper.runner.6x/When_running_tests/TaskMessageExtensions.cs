using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class TaskMessageExtensions
    {
        public static IEnumerable<TaskMessage> AssertContainsTaskStarting(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask)
        {
            var expectedTaskMessage = TaskMessage.TaskStarting(expectedTask);

            return AssertContainsTaskMessage(taskMessages, expectedTask, expectedTaskMessage);
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskFinished(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedMessage, TaskResult expectedResult)
        {
            var expectedTaskMessage = TaskMessage.TaskFinished(expectedTask, expectedMessage, expectedResult);

            return AssertContainsTaskMessage(taskMessages, expectedTask, expectedTaskMessage);
        }

        public static IEnumerable<TaskMessage> AssertContainsSuccessfulTaskFinished(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask)
        {
            return taskMessages.AssertContainsTaskFinished(expectedTask, string.Empty, TaskResult.Success);
        }

        public static IEnumerable<TaskMessage> AssertContainsTaskOutput(this IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask, string expectedOutput)
        {
            var expectedTaskMessage = TaskMessage.TaskOutput(expectedTask, expectedOutput, TaskOutputType.STDOUT);

            return AssertContainsTaskMessage(taskMessages, expectedTask, expectedTaskMessage);
        }

        private static IEnumerable<TaskMessage> AssertContainsTaskMessage(IEnumerable<TaskMessage> taskMessages, RemoteTask expectedTask,
                                                                          TaskMessage expectedTaskMessage)
        {
            var firstOrDefault = taskMessages.FirstOrDefault(tm => tm.Task == expectedTask && tm.Message == expectedTaskMessage.Message);
            if (firstOrDefault == null)
                throw new Exception(string.Format("Missing task message. Expected: {0}", expectedTaskMessage.Message));

            // Yes, thank you resharper, we do want to multiply enumerate this one
            return taskMessages;
        }
    }
}