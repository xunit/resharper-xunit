using System;
using System.Collections.Generic;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class FakeRemoteTaskServerBase
    {
        private readonly IList<TaskMessage> messages = new List<TaskMessage>();

        public IEnumerable<TaskMessage> Messages
        {
            get { return messages; }
        }

        protected void Add(TaskMessage message)
        {
            Console.WriteLine(message);
            messages.Add(message);
        }

        protected static void ReportExceptions(IEnumerable<TaskException> exceptions)
        {
            foreach (var exception in exceptions)
            {
                Console.WriteLine("{0}: {1}", exception.Type, exception.Message);
                Console.WriteLine(exception.StackTrace);
            }
        }
    }
}