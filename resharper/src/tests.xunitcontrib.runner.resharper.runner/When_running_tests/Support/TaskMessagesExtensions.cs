using System;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class TaskMessagesExtensions
    {
        public static void AssertTaskStarting(this TaskMessages taskMessages)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskStarting);
        }

        public static void AssertTaskFinished(this TaskMessages taskMessages, string message, TaskResult result)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskFinished, message, result);
        }

        public static void AssertTaskFinishedSuccessfully(this TaskMessages taskMessages)
        {
            taskMessages.AssertTaskFinished(string.Empty, TaskResult.Success);
        }

        public static void AssertTaskFinishedBadly(this TaskMessages taskMessages, Exception exception, bool infrastructure = false)
        {
            // Infrastructure exceptions (thrown when a class fails) are displayed with the short type name.
            // This is following the lead of nunit, which uses a JetBrains function to format things
            // Non-infrastructure exceptions use the full name
            Type exceptionType = exception.GetType();
            var name = infrastructure ? exceptionType.Name : exceptionType.FullName;

            taskMessages.AssertTaskFinished(name + ": " + exception.Message, TaskResult.Exception);
        }

        public static void AssertTaskDuration(this TaskMessages taskMessages, TimeSpan duration)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskDuration, duration);
        }

        public static void AssertTaskOutput(this TaskMessages taskMessages, string text)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskOutput, text, TaskOutputType.STDOUT);
        }

        public static void AssertTaskExplain(this TaskMessages taskMessages, string explanation)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskExplain, explanation);
        }

        public static void AssertTaskException(this TaskMessages taskMessages, Exception exception)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskException, exception);
        }

        public static void AssertTaskException(this TaskMessages taskMessages, params TaskException[] exceptions)
        {
            taskMessages.AssertSingleMessage(ServerAction.TaskException, exceptions);
        }

        public static void AssertCreateDynamicElement(this TaskMessages taskMessages)
        {
            taskMessages.AssertSingleMessage(ServerAction.CreateDynamicElement);
        }
    }
}