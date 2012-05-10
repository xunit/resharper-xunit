using System;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    // TODO: These tests know too much about xunit's internals
    public class When_custom_before_after_attribute_throws : SingleClassTestRunContext
    {
        [Fact]
        public void Should_fail_while_running_test()
        {
            // If a BeforeAfterAttribute throws, it's caught and rethrown by BeforeAfterCommand and then
            // caught by ExceptionAndOutputCaptureCommand, which returns a FailedResult. So this is a normal
            // failing test method
            // I don't like that this test has to know this
            var exception = new InvalidOperationException("Thrown by a custom before/after attribute");
            var method = testClass.AddFailingTest("TestMethod1", exception);

            Run();

            Messages.AssertContainsTaskStarting(method.Task);
            Messages.AssertContainsTaskException(method.Task, exception);
            Messages.AssertContainsFailedTaskFinished(method.Task, exception);
        }

        [Fact]
        public void Should_continue_running_tests()
        {
            var exception = new InvalidOperationException("Thrown by a custom before/after attribute");
            var method1 = testClass.AddFailingTest("TestMethod1", exception);
            var method2 = testClass.AddFailingTest("TestMethod2", exception);

            Run();

            Messages.AssertContainsTaskStarting(method1.Task);
            Messages.AssertContainsTaskException(method1.Task, exception);
            Messages.AssertContainsFailedTaskFinished(method1.Task, exception);

            Messages.AssertContainsTaskStarting(method2.Task);
            Messages.AssertContainsTaskException(method2.Task, exception);
            Messages.AssertContainsFailedTaskFinished(method2.Task, exception);
        }
    }
}