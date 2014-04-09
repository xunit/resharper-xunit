using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests
{
    public abstract class XunitTaskRunnerOutputTestBase : XunitTaskRunnerTestBase
    {
        protected IEnumerable<XElement> Messages;

        public override void SetUp()
        {
            base.SetUp();

            Messages = DoOneTestWithCapturedOutput(GetTestName());
        }

        protected abstract string GetTestName();

        protected TaskId ForTask(string typeName, string methodName = null, string theoryName = null)
        {
            return new TaskId(typeName, methodName, theoryName);
        }

        protected void AssertContainsOutput(TaskId task,
            string expectedOutput)
        {
            var messages = from e in Messages
                where e.Name == "task-output" && task.MatchesTaskElement(e)
                select e.Element("message").Value;
            var output = messages.Single();
            Assert.AreEqual(expectedOutput, output);
        }

        protected void AssertMessageOrder(TaskId task,
            params string[] messageTypes)
        {
            var messages = from m in Messages
                where task.MatchesTaskElement(m)
                select m.Name.ToString();
            CollectionAssert.IsSubsetOf(messageTypes, messages);
        }

        protected class TaskId
        {
            private readonly string typeName;
            private readonly string methodName;
            private readonly string theoryName;

            public TaskId(string typeName, string methodName = null, string theoryName = null)
            {
                this.typeName = typeName;
                this.methodName = methodName;
                this.theoryName = theoryName;
            }

            public bool MatchesTaskElement(XElement element)
            {
                var task = element.Element("task");
                return MatchesElement(task);
            }

            private bool MatchesElement(XElement element)
            {
                return element != null
                       && MatchesAttribute(element, "TypeName", typeName)
                       && MatchesAttribute(element, "MethodName", methodName)
                       && MatchesAttribute(element, "TheoryName", theoryName);
            }

            private static bool MatchesAttribute(XElement task,
                string attributeName, string expectedValue)
            {
                var attribute = task.Attribute(attributeName);
                if (attribute == null || string.IsNullOrEmpty(expectedValue))
                    return true;
                return attribute.Value == expectedValue;
            }
        }
    }
}