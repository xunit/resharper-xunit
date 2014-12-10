using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract class XunitTaskRunnerOutputTestBase : XunitTaskRunnerTestBase
    {
        private IList<XElement> messageElements;

        protected static class TaskAction
        {
            public const string Create = "task-create";
            public const string Start = "task-start";
            public const string Duration = "task-duration";
            public const string Output = "task-output";
            public const string Exception = "task-exception";
            public const string Finish = "task-finish";
        }

        protected static class TaskResult
        {
            public const string Exception = "Exception";
            public const string Error = "Error";
            public const string Success = "Success";
        }

        protected XunitTaskRunnerOutputTestBase(string environmentId)
            : base(environmentId)
        {
        }

        public override void SetUp()
        {
            base.SetUp();

            messageElements = DoOneTestWithCapturedOutput(GetTestName());
        }

        protected abstract string GetTestName();

        protected static TaskId ForTaskOnly(string typeName, string methodName = null, string theoryName = null)
        {
            return new TaskId(typeName, methodName, theoryName, includeChildren: false);
        }

        protected static TaskId ForTaskAndChildren(string typeName, string methodName = null, string theoryName = null)
        {
            return new TaskId(typeName, methodName, theoryName, includeChildren: true);
        }

        protected void AssertDoesNotContain(TaskId task, string action)
        {
            var messages = from e in messageElements
                where e.Name == action && task.MatchesTaskElement(e)
                select e;
            CollectionAssert.IsEmpty(messages);
        }

        protected void AssertContainsOutput(TaskId task, string expectedOutput)
        {
            var messages = from e in messageElements
                where e.Name == TaskAction.Output && task.MatchesTaskElement(e)
                select GetElementValue(e, "message").TrimEnd();
            var output = messages.Single();
            Assert.AreEqual(expectedOutput, output);
        }

        protected void AssertContainsOutput(TaskId task, params string[] expectedOutput)
        {
            var messages = from e in messageElements
                           where e.Name == TaskAction.Output && task.MatchesTaskElement(e)
                           select GetElementValue(e, "message").TrimEnd();
            CollectionAssert.AreEqual(expectedOutput, messages);
        }
        protected void AssertContainsError(TaskId task, string message)
        {
            AssertContainsFinish(task, TaskResult.Error, message);
        }

        protected void AssertContainsErrorFinal(TaskId task, string expectedMessage)
        {
            var messages = from e in messageElements
                where e.Name == TaskAction.Finish && task.MatchesTaskElement(e)
                select new
                {
                    Result = e.Attribute("result").Value,
                    Message = GetElementValue(e, "message")
                };
            var result = messages.Last();
            Assert.AreEqual(TaskResult.Error, result.Result);
            if (!string.IsNullOrEmpty(expectedMessage))
                Assert.AreEqual(expectedMessage, result.Message);
        }

        protected void AssertContainsException(TaskId task, string expectedType,
            string expectedExceptionMessage, string expectedStackTrace)
        {
            var expectedString = string.Format("{0}\n{1}\n{2}", expectedType, expectedExceptionMessage,
                expectedStackTrace);
            var messages = from e in messageElements
                where e.Name == TaskAction.Exception && task.MatchesTaskElement(e)
                select string.Format("{0}\n{1}\n{2}", e.Attribute("type").Value,
                    GetElementValue(e, "message").Replace("\n", Environment.NewLine),
                    GetElementValue(e, "stack-trace").Trim());
            var output = messages.Join("\n---\n");
            StringAssert.Contains(expectedString, output);
        }

        protected void AssertContainsStart(TaskId task)
        {
            AssertSingle(task, TaskAction.Start);
        }

        protected void AssertContainsFailingChildren(TaskId task)
        {
            AssertContainsFinish(task, TaskResult.Exception, "One or more child tests failed");
        }

        protected void AssertContainsFinish(TaskId task, string expectedTaskResult, string expectedMessage = null)
        {
            var messages = (from e in messageElements
                where e.Name == TaskAction.Finish && task.MatchesTaskElement(e)
                select new
                {
                    Result = e.Attribute("result").Value,
                    Message = GetElementValue(e, "message")
                }).ToList();
            Assert.AreEqual(1, messages.Count, "Expected single message of type {0} for task {1}", TaskAction.Finish, task);
            var result = messages.Single();
            Assert.AreEqual(expectedTaskResult, result.Result);
            if (!string.IsNullOrEmpty(expectedMessage))
                Assert.AreEqual(expectedMessage, result.Message);
        }

        protected void AssertContainsFinishFinal(TaskId task, string expectedTaskResult, string expectedMessage = null)
        {
            var messages = from e in messageElements
                           where e.Name == TaskAction.Finish && task.MatchesTaskElement(e)
                           select new
                           {
                               Result = e.Attribute("result").Value,
                               Message = GetElementValue(e, "message")
                           };
            var result = messages.Last();
            Assert.AreEqual(expectedTaskResult, result.Result);
            if (!string.IsNullOrEmpty(expectedMessage))
                Assert.AreEqual(expectedMessage, result.Message);
        }

        protected void AssertContainsCreate(TaskId taskId)
        {
            AssertSingle(taskId, TaskAction.Create);
        }

        protected void AssertSingle(TaskId task, string messageType)
        {
            var messages = (from m in messageElements
                where m.Name == messageType && task.MatchesTaskElement(m)
                select m.Name.ToString()).ToList();
            Assert.AreEqual(1, messages.Count, "Expected one item of {0} for task {1}", messageType, task);
        }

        protected void AssertMessageOrder(TaskId task, params string[] messageTypes)
        {
            var messages = from m in messageElements
                where task.MatchesTaskElement(m)
                select m.Name.ToString();
            CollectionAssert.IsSubsetOf(messageTypes, messages, "With task {0}", task);
        }

        protected void AssertMessageOrder(IDictionary<TaskId, string> taskMessages)
        {
            var allowedTaskIds = taskMessages.Keys.Distinct().ToList();
            var expectedMessages = from tm in taskMessages
                select tm.Key + "-" + tm.Value;

            var actualMessages = from m in messageElements
                where allowedTaskIds.Any(t => t.MatchesTaskElement(m))
                select GetTaskIdString(m) + "-" + m.Name;
            CollectionAssert.IsSubsetOf(expectedMessages, actualMessages);
        }

        private string GetTaskIdString(XElement element)
        {
            var task = element.Element("task");
            var typeName = GetAttributeValue(task, "TypeName");
            var methodName = GetAttributeValue(task, "MethodName");
            var theoryName = GetAttributeValue(task, "TheoryName");
            return new TaskId(typeName, methodName, theoryName, false).ToString();
        }

        private static string GetElementValue(XContainer parentElement, string elementName)
        {
            var element = parentElement.Element(elementName);
            return element == null ? string.Empty : element.Value;
        }

        private static string GetAttributeValue(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);
            return attribute == null ? string.Empty : attribute.Value;
        }

        protected int GetMessageIndex(TaskId task, string messageType)
        {
            var indices = (messageElements.Select((e, i) => new {Element = e, Index = i})
                .Where(x => x.Element.Name == messageType && task.MatchesTaskElement(x.Element))
                .Select(x => x.Index)).ToList();
            Assert.AreEqual(1, indices.Count, "Expected single message of type {0} for task {1}", messageType, task);
            return indices.Single();
        }

        protected class TaskId
        {
            private readonly string typeName;
            private readonly string methodName;
            private readonly string theoryName;
            private readonly bool includeChildren;

            public TaskId(string typeName, string methodName, string theoryName, bool includeChildren)
            {
                this.typeName = typeName;
                this.methodName = methodName;
                this.theoryName = theoryName;
                this.includeChildren = includeChildren;
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

            private bool MatchesAttribute(XElement task,
                string attributeName, string expectedValue)
            {
                var attribute = task.Attribute(attributeName);
                if (attribute == null)
                    return string.IsNullOrEmpty(expectedValue);
                if (string.IsNullOrEmpty(expectedValue))
                    return includeChildren;
                return attribute.Value == expectedValue;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Task:<{0}>", typeName);
                if (!string.IsNullOrEmpty(methodName))
                {
                    sb.AppendFormat(":<{0}>", methodName);
                    if (!string.IsNullOrEmpty(theoryName))
                        sb.AppendFormat(":<{0}>", theoryName);
                }

                return sb.ToString();
            }
        }
    }
}