using System;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_class_throws_in_dispose : SingleClassTestRunContext
    {
        private readonly InvalidOperationException exception;
        private readonly Method method;

        public When_class_throws_in_dispose()
        {
            // Dispose throws from inside LifetimeCommand and is caught by ExceptionsAndOutputCaptureCommand,
            // which returns a FailedResult FOR THE METHOD. All of this happens in the scope of running the
            // method, so this scenario is just a normal failing test method, it just happens that the exception
            // is thrown from the class's Dispose method, and not from the method itself
            exception = new InvalidOperationException("Thrown by the class dispose method");
            testClass.SetDispose(() => { throw exception; });
            method = testClass.AddPassingTest("TestMethod1");
        }

        [Fact]
        public void Should_fail_test_as_normal_exception()
        {
            Run();

            Messages.OfTask(method.Task).AssertOrderedActions(ServerAction.TaskStarting, ServerAction.TaskException, ServerAction.TaskFinished);
        }

        [Fact]
        public void Should_report_exception()
        {
            Run();

            Messages.OfTask(method.Task).TaskException(exception);
        }

        [Fact]
        public void Should_report_failed_test_finished()
        {
            Run();

            Messages.OfTask(method.Task).TaskFinishedBadly(exception);
        }

        [Fact]
        public void Should_continue_running_tests()
        {
            var method2 = testClass.AddPassingTest("TestMethod2");

            Run();

            Messages.OfTask(method.Task).AssertOrderedActions(ServerAction.TaskStarting, ServerAction.TaskException, ServerAction.TaskFinished);
            Messages.OfTask(method2.Task).AssertOrderedActions(ServerAction.TaskStarting, ServerAction.TaskException, ServerAction.TaskFinished);
        }

        [Fact]
        public void Should_notify_class_as_successful_after_successfully_reporting_failing_tests()
        {
            // Even though the class is throwing the exception, the class is being disposed as
            // part of the lifecycle of a method test, so the exception is purely contained to
            // the method. The class itself succeeds
            Run();

            Messages.OfTask(testClass.ClassTask).TaskFinishedSuccessfully();
        }
    }
}