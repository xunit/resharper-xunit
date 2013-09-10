using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class TaskMessages
    {
        private readonly RemoteTask task;
        private readonly string taskMatchStyle;

        public TaskMessages(RemoteTask task, string taskMatchStyle, IList<TaskMessage> taskMessages)
        {
            this.task = task;
            this.taskMatchStyle = taskMatchStyle;
            Messages = taskMessages;
        }

        public IEnumerable<TaskMessage> Messages { get; private set; }
        public IEnumerable<string> MessagesText { get { return Messages.Select(tm => tm.Message); } }

        public void Assert(Action action)
        {
            Do(action);
        }

        private void Do(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw new AssertException(string.Format("With task ({3}): {0}{1}{2}", task, Environment.NewLine,
                    ex.Message, taskMatchStyle));
            }
        }
    }

    public static class TaskMessagesExtensions
    {
        public static void OrderedActions(this TaskMessages taskMessages, params ServerAction[] serverActions)
        {
            taskMessages.Assert(() =>
            {
                int i = 0;
                using (var enumerator = taskMessages.MessagesText.GetEnumerator())
                {
                    while (enumerator.MoveNext() && i < serverActions.Length)
                    {
                        var taskMessage = enumerator.Current;
                        if (taskMessage != null && taskMessage.StartsWith(serverActions[i].ToString()))
                            i++;
                    }

                    if (i < serverActions.Length)
                        throw new AssertException(string.Format("Could not find message: {0}", serverActions[i]));
                }
            });
        }

        public static void TaskStarting(this TaskMessages taskMessages)
        {
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, ServerMessage.TaskStarting()));
        }

        public static void TaskFinished(this TaskMessages taskMessages, string message, TaskResult result)
        {
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, ServerMessage.TaskFinished(message, result)));
        }

        public static void TaskFinished(this TaskMessages taskMessages)
        {
            taskMessages.TaskFinished(string.Empty, TaskResult.Success);
        }

        public static void TaskFinished(this TaskMessages taskMessages, Exception exception, bool infrastructure = false)
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
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, ServerMessage.TaskDuration(duration)));
        }

        public static void TaskOutput(this TaskMessages taskMessages, string text)
        {
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, ServerMessage.TaskOutput(text, TaskOutputType.STDOUT)));
        }

        public static void TaskExplain(this TaskMessages taskMessages, string explanation)
        {
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, ServerMessage.TaskExplain(explanation)));
        }

        public static void TaskException(this TaskMessages taskMessages, Exception exception)
        {
            string expectedMessage = ServerMessage.TaskException(exception);
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, m => m.StartsWith(expectedMessage)));
        }

        public static void TaskException(this TaskMessages taskMessages, params TaskException[] exception)
        {
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, ServerMessage.TaskException(exception)));
        }

        public static void CreateDynamicElement(this TaskMessages taskMessages)
        {
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, ServerMessage.CreateDynamicElement()));
        }
    }
}