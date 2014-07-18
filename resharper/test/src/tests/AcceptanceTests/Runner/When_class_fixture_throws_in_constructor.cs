using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [Category("xunit2")]
    public class When_class_fixture_throws_in_constructor : XunitTaskRunnerOutputTestBase
    {
        private const string ExceptionType = "System.InvalidOperationException";
        private const string ExceptionMessage = "Thrown in fixture Constructor";
        private const string StackTrace = "at Foo.Fixture..ctor()";

        public When_class_fixture_throws_in_constructor()
            : base("xunit2")
        {
        }

        protected override string GetTestName()
        {
            return "ClassFixtureThrowsInConstructor";
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
        public void Should_not_notify_class_exception()
        {
            // Because the failure is due to the methods failing, not the class itself
            // (Error in ctor is reported to method. Error in dispose to class)
            AssertDoesNotContain(ClassTaskId, TaskAction.Exception);
        }

        [Test]
        public void Should_notify_class_failed()
        {
            AssertContainsFinish(ClassTaskId, TaskResult.Exception, "One or more child tests failed");
        }

        [Test]
        public void Should_notify_all_methods_as_finished_and_failed()
        {
            // TODO: Not sure if we should strip out AggregateException
            // Tempted to leave it, because more than one error did occur
            // i.e. unable to create fixture, and unable to create class (due to fixture)
            const string message = "System.AggregateException: One or more errors occurred.";

            AssertContainsException(Method1TaskId, ExceptionType, ExceptionMessage, StackTrace);
            AssertContainsFinishFinal(Method1TaskId, TaskResult.Exception, message);
            AssertContainsException(Method2TaskId, ExceptionType, ExceptionMessage, StackTrace);
            AssertContainsFinishFinal(Method2TaskId, TaskResult.Exception, message);
        }

        [Test]
        public void Should_continue_running_other_classes()
        {
            AssertContainsStart(Class2TaskId);
        }
    }
}