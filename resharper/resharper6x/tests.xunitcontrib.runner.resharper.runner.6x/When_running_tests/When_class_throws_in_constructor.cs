using System;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_class_throws_in_constructor : SingleClassTestRunContext
    {
        [Fact]
        public void Should_fail_while_running_test()
        {
            // Constructor throws. LifetimeCommand catches and unwraps TargetInvocationException
            // ExceptionAndOutputCaptureCommand catches and returns a FailedResult. So this is a
            // normal failing test method. It just fails with the exception that's thrown in the
            // constructor, rather than the method itself.
            var exception = new InvalidOperationException("Thrown by the class constructor");
            testClass.SetConstructor(() => { throw exception; });
            var method = testClass.AddPassingTest("TestMethod1");

            Run();

            Messages.AssertContainsTaskStarting(method.Task);
            Messages.AssertContainsTaskException(method.Task, exception);
            Messages.AssertContainsFailedTaskFinished(method.Task, exception);
        }

        [Fact]
        public void Should_continue_running_tests()
        {
            var exception = new InvalidOperationException("Thrown by the class constructor");
            testClass.SetConstructor(() => { throw exception; });
            var method1 = testClass.AddPassingTest("TestMethod1");
            var method2 = testClass.AddPassingTest("TestMethod2");

            Run();

            Messages.AssertContainsTaskStarting(method1.Task);
            Messages.AssertContainsTaskException(method1.Task, exception);
            Messages.AssertContainsFailedTaskFinished(method1.Task, exception);

            Messages.AssertContainsTaskStarting(method2.Task);
            Messages.AssertContainsTaskException(method2.Task, exception);
            Messages.AssertContainsFailedTaskFinished(method2.Task, exception);
        }

        [Fact]
        public void Should_notify_class_as_running_successfully_even_though_run_had_errors()
        {
            var exception = new InvalidOperationException("Thrown by the class constructor");
            testClass.SetConstructor(() => { throw exception; });
            testClass.AddPassingTest("TestMethod1");

            Run();

            Messages.AssertContainsSuccessfulTaskFinished(testClass.ClassTask);
        }
    }
}