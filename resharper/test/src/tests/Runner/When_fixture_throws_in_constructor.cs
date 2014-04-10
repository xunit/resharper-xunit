using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class When_fixture_throws_in_constructor : XunitTaskRunnerOutputTestBase
    {
        private const string ExceptionType = "System.InvalidOperationException";
        private const string ExceptionMessage = "Thrown in fixture constructor";
        private const string StackTrace = "at Foo.Fixture..ctor()";

        protected override string GetTestName()
        {
            return "FixtureThrowsInConstructor";
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

    public class When_fixture_throws_in_dispose : XunitTaskRunnerOutputTestBase
    {
        private const string ExceptionType = "System.InvalidOperationException";
        private const string ExceptionMessage = "Thrown in fixture Dispose";
        private const string StackTrace = "at Foo.Fixture.Dispose()";

        protected override string GetTestName()
        {
            return "FixtureThrowsInDispose";
        }

        private TaskId ClassTaskId
        {
            get { return ForTaskOnly("Foo.FixtureThrowsInDispose"); }
        }

        private TaskId Class2TaskId
        {
            get { return ForTaskOnly("Foo.Class2"); }
        }

        private TaskId Method1TaskId
        {
            get { return ForTaskOnly("Foo.FixtureThrowsInDispose", "TestMethod1"); }
        }

        private TaskId Method2TaskId
        {
            get { return ForTaskOnly("Foo.FixtureThrowsInDispose", "TestMethod2"); }
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
            const string message = "Class failed in Foo.FixtureThrowsInDispose";

            AssertContainsException(Method1TaskId, string.Empty, message, string.Empty);
            AssertContainsErrorFinal(Method1TaskId, message);
            AssertContainsException(Method2TaskId, string.Empty, message, string.Empty);
            AssertContainsErrorFinal(Method2TaskId, message);
        }

        [Test]
        public void Should_continue_running_other_classes()
        {
            AssertContainsStart(Class2TaskId);
        }
    }

}