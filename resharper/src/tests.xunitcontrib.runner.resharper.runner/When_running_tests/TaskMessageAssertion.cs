using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class TaskMessageAssertion
    {
        private readonly RemoteTask task;
        private readonly string taskMatchStyle;
        private readonly IList<TaskMessage> messages;

        public TaskMessageAssertion(RemoteTask task, string taskMatchStyle, IList<TaskMessage> messages)
        {
            this.task = task;
            this.taskMatchStyle = taskMatchStyle;
            this.messages = messages;
        }

        private IEnumerable<string> Messages
        {
            get { return messages.Select(m => m.Message); }
        }

        private void Do(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw new AssertException(string.Format("With task ({3}): {0}{1}{2}", task, Environment.NewLine, ex.Message, taskMatchStyle));
            }
        }

        public void OrderedActions(params ServerAction[] serverActions)
        {
            Do(() =>
                {
                    int i = 0;
                    using (var enumerator = messages.GetEnumerator())
                    {
                        while (enumerator.MoveNext() && i < serverActions.Length)
                        {
                            var taskMessage = enumerator.Current;
                            if (taskMessage != null && taskMessage.Message.StartsWith(serverActions[i].ToString()))
                                i++;
                        }

                        if (i < serverActions.Length)
                            throw new AssertException(string.Format("Could not find message: {0}", serverActions[i]));
                    }
                });
        }

        public void TaskStarting()
        {
            Do(() => Assert.Single(Messages, ServerMessage.TaskStarting()));
        }

        public void TaskFinished(string message, TaskResult result)
        {
            Do(() => Assert.Single(Messages, ServerMessage.TaskFinished(message, result)));
        }

        public void TaskFinished()
        {
            TaskFinished(string.Empty, TaskResult.Success);
        }

        public void TaskFinished(Exception exception, bool infrastructure = false)
        {
            // Infrastructure exceptions (thrown when a class fails) are displayed with the short type name.
            // This is following the lead of nunit, which uses a JetBrains function to format things
            // Non-infrastructure exceptions use the full name
            Type exceptionType = exception.GetType();
            var name = infrastructure ? exceptionType.Name : exceptionType.FullName;

            TaskFinished(name + ": " + exception.Message, TaskResult.Exception);
        }

        public void TaskDuration(TimeSpan duration)
        {
            Do(() => Assert.Single(Messages, ServerMessage.TaskDuration(duration)));
        }

        public void TaskOutput(string text)
        {
            Do(() => Assert.Single(Messages, ServerMessage.TaskOutput(text, TaskOutputType.STDOUT)));
        }

        public void TaskExplain(string explanation)
        {
            Do(() => Assert.Single(Messages, ServerMessage.TaskExplain(explanation)));
        }

        public void TaskException(Exception exception)
        {
            string expectedMessage = ServerMessage.TaskException(exception);
            Do(() => Assert.Single(Messages, m => m.StartsWith(expectedMessage)));
        }

        public void TaskException(params TaskException[] exception)
        {
            Do(() => Assert.Single(Messages, ServerMessage.TaskException(exception)));
        }

        public void CreateDynamicElement()
        {
            Do(() => Assert.Single(Messages, ServerMessage.CreateDynamicElement()));
        }
    }
}