using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    // TODO: This only tests an exception in Before, not After
    public class When_custom_before_after_attribute_throws : XunitTaskRunnerOutputTestBase
    {
        public When_custom_before_after_attribute_throws(string environmentId)
            : base(environmentId)
        {
        }

        protected override string GetTestName()
        {
            return "CustomBeforeAfterAttributeThrows";
        }

        private TaskId Method1TaskId
        {
            get { return ForTaskOnly("Foo.CustomBeforeAfterThrows", "TestMethod1"); }
        }

        private TaskId Method2TaskId
        {
            get { return ForTaskOnly("Foo.CustomBeforeAfterThrows", "TestMethod2"); }
        }

        [Test]
        public void Should_fail_test_as_normal_exception()
        {
            AssertMessageOrder(Method1TaskId,
                TaskAction.Start,
                    TaskAction.Exception,
                TaskAction.Finish);
        }

        [Test]
        public void Should_report_exception()
        {
            AssertContainsException(Method1TaskId, "System.InvalidOperationException",
                "Thrown in Before", "at Foo.CustomBeforeAfterThrows.CustomBeforeAfterTestAttribute.Before(MethodInfo methodUnderTest)");
        }

        [Test]
        public void Should_report_failed_test_finished()
        {
            AssertContainsFinish(Method1TaskId, TaskResult.Exception,
                "System.InvalidOperationException: Thrown in Before");
        }

        [Test]
        public void Should_continue_running_tests()
        {
            AssertMessageOrder(Method1TaskId, TaskAction.Start, TaskAction.Exception, TaskAction.Finish);
            AssertMessageOrder(Method2TaskId, TaskAction.Start, TaskAction.Finish);
        }
    }
}