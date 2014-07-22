using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [Category("xunit2")]
    public class When_collection_fixture_throws_in_dispose : XunitTaskRunnerOutputTestBase
    {
        private const string ExceptionType = "System.InvalidOperationException";
        private const string ExceptionMessage = "Thrown in fixture Dispose";
        private const string StackTrace = "at Foo.Fixture.Dispose()";

        public When_collection_fixture_throws_in_dispose()
            : base("xunit2")
        {
        }

        protected override string GetTestName()
        {
            return "CollectionFixtureThrowsInDispose";
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
            AssertContainsFinishFinal(ClassTaskId, TaskResult.Exception, ExceptionType + ": " + ExceptionMessage);
        }

        [Test]
        public void Should_notify_all_methods_as_finished_and_failed()
        {
            const string message = "Collection cleanup failed in My collection";

            AssertContainsException(Method1TaskId, string.Empty, message, string.Empty);
            AssertContainsErrorFinal(Method1TaskId, message);
            AssertContainsException(Method2TaskId, string.Empty, message, string.Empty);
            AssertContainsErrorFinal(Method2TaskId, message);
        }

        [Test]
        public void Should_continue_running_other_collections()
        {
            AssertContainsStart(Class2TaskId);
        }
    }
}