using System;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_class_throws_in_constructor : SingleClassTestRunContext
    {
        private readonly InvalidOperationException exception;
        private readonly Method method;

        public When_class_throws_in_constructor()
        {
            // Constructor throws. LifetimeCommand catches and unwraps TargetInvocationException
            // ExceptionAndOutputCaptureCommand catches and returns a FailedResult FOR THE METHOD.
            // All of this happens in the scope of running the method, so this scenario is just
            // a normal failing test method, it just happens that the exception is thrown from
            // the class constructor, before the method has a chance to execute
            exception = new InvalidOperationException("Thrown by the class constructor");
            testClass.SetConstructor(() => { throw exception; });
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

            Messages.OfTask(method.Task).AssertTaskException(exception);
        }

        [Fact]
        public void Should_report_failed_test_exception()
        {
            Run();

            Messages.OfTask(method.Task).AssertTaskFinishedBadly(exception);
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
        public void Should_notify_class_as_running_successfully_even_though_run_had_errors()
        {
            // Even though the class is throwing the exception, the class is being created as
            // part of the lifecycle of a method test, so the exception is purely contained to
            // the method. The class itself succeeds
            Run();

            Messages.OfTask(testClass.ClassTask).AssertTaskFinishedSuccessfully();
        }
    }
}