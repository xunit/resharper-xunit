using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests
{
    public abstract class XunitTaskRunnerOutputTestBase : XunitTaskRunnerTestBase
    {
        private IEnumerable<XElement> messageElements;

        protected static class TaskAction
        {
            public const string Start = "task-start";
            public const string Duration = "task-duration";
            public const string Output = "task-output";
            public const string Exception = "task-exception";
            public const string Finish = "task-finish";
        }

        protected static class TaskResult
        {
            public const string Exception = "Exception";
        }

        public override void SetUp()
        {
            base.SetUp();

            messageElements = DoOneTestWithCapturedOutput(GetTestName());
        }

        protected abstract string GetTestName();

        protected TaskId ForTask(string typeName, string methodName = null, string theoryName = null)
        {
            return new TaskId(typeName, methodName, theoryName);
        }

        protected void AssertContainsOutput(TaskId task,
            string expectedOutput)
        {
            var messages = from e in messageElements
                where e.Name == TaskAction.Output && task.MatchesTaskElement(e)
                select GetElementValue(e, "message");
            var output = messages.Single();
            Assert.AreEqual(expectedOutput, output);
        }

        protected void AssertContainsException(TaskId task, string expectedType,
            string expectedExceptionMessage, string expectedStackTrace)
        {
            var messages = from e in messageElements
                where e.Name == TaskAction.Exception && task.MatchesTaskElement(e)
                select new
                {
                    Type = e.Attribute("type").Value,
                    Message = GetElementValue(e, "message"),
                    StackTrace = GetElementValue(e, "stack-trace")
                };
            var output = messages.Single();
            Assert.AreEqual(expectedType, output.Type);
            Assert.AreEqual(expectedExceptionMessage, output.Message.Replace("\n", Environment.NewLine));
            Assert.AreEqual(expectedStackTrace, output.StackTrace.Trim());
        }

        protected void AssertContainsFinish(TaskId task, string expectedTaskResult)
        {
            var messages = from e in messageElements
                where e.Name == TaskAction.Finish && task.MatchesTaskElement(e)
                select e.Attribute("result").Value;
            var result = messages.Single();
            Assert.AreEqual(expectedTaskResult, result);
        }

        protected void AssertMessageOrder(TaskId task,
            params string[] messageTypes)
        {
            var messages = from m in this.messageElements
                where task.MatchesTaskElement(m)
                select m.Name.ToString();
            CollectionAssert.IsSubsetOf(messageTypes, messages);
        }

        private static string GetElementValue(XContainer parentElement, string elementName)
        {
            var element = parentElement.Element(elementName);
            return element == null ? string.Empty : element.Value;
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
                if (string.IsNullOrEmpty(expectedValue))
                    return true;

                var attribute = task.Attribute(attributeName);
                if (attribute == null)
                    return false;
                return attribute.Value == expectedValue;
            }
        }
    }
}