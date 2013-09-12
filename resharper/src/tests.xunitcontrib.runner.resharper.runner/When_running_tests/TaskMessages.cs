using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class TaskMessages
    {
        private readonly RemoteTask task;
        private readonly string taskMatchStyle;
        private readonly IEnumerable<string> messages;
        private readonly IEnumerable<ServerAction> serverActions;

        public TaskMessages(RemoteTask task, string taskMatchStyle, IList<TaskMessage> taskMessages)
        {
            this.task = task;
            this.taskMatchStyle = taskMatchStyle;
            messages = taskMessages.Select(tm => tm.Message).ToList();
            serverActions = taskMessages.Select(tm => tm.ServerAction).ToList();
        }

        private void Assert(Action action)
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

        public void AssertSingleMessage(ServerAction expectedServerAction, params object[] parameters)
        {
            var expectedServerMessage = ServerMessage.Format(expectedServerAction, parameters);
            Assert(() => Xunit.Assert.Single(messages, expectedServerMessage));
        }

        public void AssertNoAction(ServerAction expectedServerAction)
        {
            Assert(() => Xunit.Assert.DoesNotContain(expectedServerAction, serverActions));
        }

        public void AssertNoMessages()
        {
            Assert(() => Xunit.Assert.Empty(messages));
        }

        public void AssertOrderedActions(params ServerAction[] actions)
        {
            Assert(() =>
            {
                int i = 0;
                using (var enumerator = serverActions.GetEnumerator())
                {
                    while (enumerator.MoveNext() && i < actions.Length)
                    {
                        var serverAction = enumerator.Current;
                        if (serverAction == actions[i])
                            i++;
                    }

                    if (i < actions.Length)
                        throw new AssertException(string.Format("Could not find message: {0}", actions[i]));
                }
            });
        }
    }
}