using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;
using Assert = Xunit.Assert;

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

        public void AssertMessage(string expectedTaskMessage)
        {
            Assert(() => Xunit.Assert.Single(MessagesText, expectedTaskMessage));
        }

        public void AssertSingleAction(ServerAction action)
        {
            Assert(() =>
            {
                var actionText = action.ToString();
                try
                {
                    Assert(() => Xunit.Assert.Single(MessagesText, m => m.StartsWith(actionText)));
                }
                catch (Exception e)
                {
                    throw new AssertException(string.Format("Looking for message starting with: {0}", actionText));
                }
            });
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
            taskMessages.AssertMessage(ServerMessage.TaskStarting());
        }

        public static void TaskFinished(this TaskMessages taskMessages, string message, TaskResult result)
        {
            taskMessages.AssertMessage(ServerMessage.TaskFinished(message, result));
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
            taskMessages.AssertMessage(ServerMessage.TaskDuration(duration));
        }

        public static void TaskOutput(this TaskMessages taskMessages, string text)
        {
            taskMessages.AssertMessage(ServerMessage.TaskOutput(text, TaskOutputType.STDOUT));
        }

        public static void TaskExplain(this TaskMessages taskMessages, string explanation)
        {
            taskMessages.AssertMessage(ServerMessage.TaskExplain(explanation));
        }

        public static void TaskException(this TaskMessages taskMessages, Exception exception)
        {
            // Exception doesn't contain a stack trace
            string expectedMessage = ServerMessage.TaskException(exception);
            taskMessages.Assert(() => Assert.Single(taskMessages.MessagesText, m => m.StartsWith(expectedMessage)));
        }

        public static void TaskException(this TaskMessages taskMessages, params TaskException[] exception)
        {
            taskMessages.AssertMessage(ServerMessage.TaskException(exception));
        }

        public static void CreateDynamicElement(this TaskMessages taskMessages)
        {
            taskMessages.AssertMessage(ServerMessage.CreateDynamicElement());
        }
    }
}