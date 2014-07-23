using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [Category("xunit1")]
    public class When_fixture_throws_in_constructor_xunit1 : XunitTaskRunnerOutputTestBase
    {
        private const string ExceptionType = "System.InvalidOperationException";
        private const string ExceptionMessage = "Thrown in fixture constructor";
        private const string StackTrace = "at Foo.Fixture..ctor()";

        public When_fixture_throws_in_constructor_xunit1()
            : base("xunit1")
        {
        }

        protected override string GetTestName()
        {
            return "FixtureThrowsInConstructor.xunit1";
        }

        private TaskId ClassTaskId
        {
            get { return ForTaskOnly("Foo.FixtureThrowsInConstructor"); }
        }

        private TaskId Class2TaskId
        {
            get { return ForTaskOnly("Foo.Class2"); }
        }

        private TaskId Method1TaskId
        {
            get { return ForTaskOnly("Foo.FixtureThrowsInConstructor", "TestMethod1"); }
        }

        private TaskId Method2TaskId
        {
            get { return ForTaskOnly("Foo.FixtureThrowsInConstructor", "TestMethod2"); }
        }

        [Test]
        public void Should_notify_class_start()
        {
            AssertContainsStart(ClassTaskId);
        }

        [Test]
        public void Should_notify_class_exception()
        {
            AssertContainsException(ClassTaskId, ExceptionType, ExceptionMessage, StackTrace);
        }

        [Test]
        public void Should_notify_class_failed()
        {
            AssertContainsFinish(ClassTaskId, TaskResult.Exception, ExceptionType + ": " + ExceptionMessage);
        }

        [Test]
        public void Should_notify_all_methods_as_finished_and_failed()
        {
            const string message = "Class failed in Foo.FixtureThrowsInConstructor";

            AssertContainsException(Method1TaskId, string.Empty, message, string.Empty);
            AssertContainsError(Method1TaskId, message);
            AssertContainsException(Method2TaskId, string.Empty, message, string.Empty);
            AssertContainsError(Method2TaskId, message);
        }

        [Test]
        public void Should_continue_running_other_classes()
        {
            AssertContainsStart(Class2TaskId);
        }
    }
}