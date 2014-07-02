using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    public class When_a_fact_fails : XunitTaskRunnerOutputTestBase
    {
        public When_a_fact_fails(string environment)
            : base(environment)
        {
        }

        protected override string GetTestName()
        {
            return "FailingFact";
        }

        private TaskId ClassTaskId
        {
            get { return ForTaskOnly("Foo.FailingFact"); }
        }

        private TaskId TaskId
        {
            get { return ForTaskAndChildren("Foo.FailingFact", "Fails"); }
        }

        [Test]
        public void Should_notify_method_exception()
        {
            // TODO: Get this directly from xunit? How does that work with xunit2?
            // TODO: Do I care about this? Could be handled by the gold files?
            const string assertMessage = @"Assert.Equal() Failure
Expected: 12
Actual:   42";
            const string stackTrace = "at Foo.FailingFact.Fails()";

            AssertContainsException(TaskId, "Xunit.Sdk.EqualException", assertMessage, stackTrace);
        }

        [Test]
        public void Should_notify_method_finished_with_errors()
        {
            AssertContainsFinish(TaskId, TaskResult.Exception);
        }

        [Test]
        public void Should_notify_exception_before_method_finished()
        {
            AssertMessageOrder(TaskId, TaskAction.Start, TaskAction.Exception, TaskAction.Finish);
        }

        [Test]
        public void Should_notify_class_finished_with_errors()
        {
            AssertContainsFinish(ClassTaskId, TaskResult.Exception, "One or more child tests failed");
        }
    }
}