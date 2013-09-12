using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;
using System.Linq;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class TaskMessageExtensions
    {
        public static TaskMessages OfTask(this IEnumerable<TaskMessage> taskMessages, RemoteTask task)
        {
            var sameTasks = from tm in taskMessages
                where Equals(tm.Task, task) && tm.Task.Id == task.Id
                select tm;
            return new TaskMessages(task, "same", sameTasks.ToList());
        }

        public static TaskMessages OfEquivalentTask(this IEnumerable<TaskMessage> taskMessages, RemoteTask task)
        {
            var equivalentTasks = from tm in taskMessages
                where Equals(tm.Task, task)
                select tm;
            return new TaskMessages(task, "equal", equivalentTasks.ToList());
        }

        public static void AssertOrderedSubsetWithSameTasks(this IEnumerable<TaskMessage> taskMessages, TaskMessage[] expectedMessages)
        {
            int i = 0;
            using (var enumerator = taskMessages.GetEnumerator())
            {
                while (enumerator.MoveNext() && i < expectedMessages.Length)
                {
                    var taskMessage = enumerator.Current;
                    if (taskMessage != null && Equals(taskMessage.Task, expectedMessages[i].Task) && taskMessage.Task.Id == expectedMessages[i].Task.Id && taskMessage.Message == expectedMessages[i].Message)
                        i++;
                }

                if (i < expectedMessages.Length)
                    throw new AssertException(string.Format("Could not find message: {0} - {1}", expectedMessages[i].Task, expectedMessages[i].Message));
            }
        }
    }
}