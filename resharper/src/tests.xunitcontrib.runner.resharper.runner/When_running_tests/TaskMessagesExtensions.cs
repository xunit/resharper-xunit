using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class TaskMessagesExtensions
    {
        public static void TaskStarting(this TaskMessages taskMessages)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskStarting);
        }

        public static void TaskFinished(this TaskMessages taskMessages, string message, TaskResult result)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskFinished, message, result);
        }

        public static void TaskFinishedSuccessfully(this TaskMessages taskMessages)
        {
            taskMessages.TaskFinished(string.Empty, TaskResult.Success);
        }

        public static void TaskFinishedBadly(this TaskMessages taskMessages, Exception exception, bool infrastructure = false)
        {
            // Infrastructure exceptions (thrown when a class fails) are displayed with the short type name.
            // This is following the lead of nunit, which uses a JetBrains function to format things
            // Non-infrastructure exceptions use the full name
            Type exceptionType = exception.GetType();
            var name = infrastructure ? exceptionType.Name : exceptionType.FullName;

            taskMessages.TaskFinished(name + ": " + exception.Message, TaskResult.Exception);
        }

        public static void TaskDuration(this TaskMessages taskMessages, TimeSpan duration)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskDuration, duration);
        }

        public static void TaskOutput(this TaskMessages taskMessages, string text)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskOutput, text, TaskOutputType.STDOUT);
        }

        public static void TaskExplain(this TaskMessages taskMessages, string explanation)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskExplain, explanation);
        }

        public static void TaskException(this TaskMessages taskMessages, Exception exception)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskException, exception);
        }

        public static void TaskException(this TaskMessages taskMessages, params TaskException[] exceptions)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskException, exceptions);
        }

        public static void CreateDynamicElement(this TaskMessages taskMessages)
        {
            taskMessages.AssertSingleMessage(ServerAction.CreateDynamicElement);
        }
    }
}