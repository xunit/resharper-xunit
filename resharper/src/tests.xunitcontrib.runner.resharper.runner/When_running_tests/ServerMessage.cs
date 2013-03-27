using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class ServerMessage
    {
        public static string TaskStarting()
        {
            return ServerAction.TaskStarting.ToString();
        }

        public static string TaskProgress(string message)
        {
            return string.Format("{0}: {1}", ServerAction.TaskProgress, message);
        }

        public static string TaskError(string message)
        {
            return string.Format("{0}: {1}", ServerAction.TaskError, message);
        }

        public static string TaskOutput(string text, TaskOutputType outputType)
        {
            return string.Format("{0}: {1} - <{2}>", ServerAction.TaskOutput, outputType, text);
        }

        public static string TaskFinished(string message, TaskResult result)
        {
            return string.Format("{0}: {1} - <{2}>", ServerAction.TaskFinished, result, message);
        }

        public static string TaskDuration(TimeSpan duration)
        {
            return string.Format("{0}: {1}", ServerAction.TaskDuration, duration);
        }

        public static string TaskExplain(string explanation)
        {
            return string.Format("{0}: {1}", ServerAction.TaskExplain, explanation);
        }

        public static string TaskException(IEnumerable<TaskException> exceptions)
        {
            var formattedExceptions = from exception in exceptions
                                      select FormatException(exception.Type, exception.Message, exception.StackTrace);
            var exceptionText = string.Join(Environment.NewLine, formattedExceptions.ToArray());
            return TaskException(exceptionText);
        }

        public static string TaskException(Exception exception)
        {
            var exceptions = new List<string>();
            while (exception != null)
            {
                exceptions.Add(FormatException(exception.GetType().FullName, exception.Message, exception.StackTrace));
                exception = exception.InnerException;
            }

            var exceptionText = string.Join(Environment.NewLine, exceptions.ToArray());

            return TaskException(exceptionText);
        }

        private static string FormatException(string type, string message, string stackTrace)
        {
            return string.Format("  -> {0}: {1} {2}", type, message, stackTrace);
        }

        public static string TaskException(string exceptionText)
        {
            return string.Format("{0}:{1}{2}", ServerAction.TaskException, Environment.NewLine, exceptionText);
        }

        public static string CreateDynamicElement()
        {
            return ServerAction.CreateDynamicElement.ToString();
        }
    }
}