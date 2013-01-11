using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Sdk;
using System.Linq;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public static class TaskMessageExtensions
    {
        public static IEnumerable<TaskMessage> ForEqualTask(this IEnumerable<TaskMessage> taskMessages, RemoteTask task)
        {
            return taskMessages.Where(m => Equals(m.Task, task));
        }

        public static IEnumerable<TaskMessage> ForSameTask(this IEnumerable<TaskMessage> taskMessages, RemoteTask task)
        {
            return taskMessages.Where(m => Equals(m.Task, task) && m.Task.Id == task.Id);
        }

        public static TaskMessageAssertion AssertSameTask(this IEnumerable<TaskMessage> taskMessages, RemoteTask task)
        {
            return new TaskMessageAssertion(task, "same", taskMessages.ForSameTask(task).ToList());
        }

        public static TaskMessageAssertion AssertEqualTask(this IEnumerable<TaskMessage> taskMessages, RemoteTask task)
        {
            return new TaskMessageAssertion(task, "equal", taskMessages.ForEqualTask(task).ToList());
        }

        public static void AssertOrderWithSameTasks(this IEnumerable<TaskMessage> taskMessages, TaskMessage[] expectedMessages)
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