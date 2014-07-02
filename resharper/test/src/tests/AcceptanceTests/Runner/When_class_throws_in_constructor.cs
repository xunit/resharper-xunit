using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    public class When_class_throws_in_constructor : XunitTaskRunnerOutputTestBase
    {
        public When_class_throws_in_constructor(string environmentId)
            : base(environmentId)
        {
        }

        protected override string GetTestName()
        {
            return "ClassThrowsInConstructor";
        }

        private TaskId ClassTaskId
        {
            get { return ForTaskOnly("Foo.ClassThrowsInConstructor"); }
        }

        private TaskId Method1TaskId
        {
            get { return ForTaskOnly("Foo.ClassThrowsInConstructor", "TestMethod1"); }
        }

        private TaskId Method2TaskId
        {
            get { return ForTaskOnly("Foo.ClassThrowsInConstructor", "TestMethod2"); }
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
                "Thrown from constructor", "at Foo.ClassThrowsInConstructor..ctor()");
        }

        [Test]
        public void Should_report_failed_test_exception()
        {
            AssertContainsFinish(Method1TaskId, TaskResult.Exception, "System.InvalidOperationException: Thrown from constructor");
        }

        [Test]
        public void Should_continue_running_tests()
        {
            AssertMessageOrder(Method1TaskId, TaskAction.Start, TaskAction.Exception, TaskAction.Finish);
            AssertMessageOrder(Method2TaskId, TaskAction.Start, TaskAction.Exception, TaskAction.Finish);
        }

        [Test]
        public void Should_fail_class_due_to_failing_children()
        {
            AssertContainsFailingChildren(ClassTaskId);
        }
    }
}