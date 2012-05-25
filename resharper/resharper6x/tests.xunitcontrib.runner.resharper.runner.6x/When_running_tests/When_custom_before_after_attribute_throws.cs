using System;
using System.Reflection;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_custom_before_after_attribute_throws : SingleClassTestRunContext
    {
        [Fact]
        public void Should_fail_while_running_test()
        {
            // If a BeforeAfterAttribute throws, it's caught and rethrown by BeforeAfterCommand and then
            // caught by ExceptionAndOutputCaptureCommand, which returns a FailedResult. So this is a normal
            // failing test method
            var exception = new InvalidOperationException("Thrown by a custom before/after attribute");

            var method = testClass.AddMethod("TestMethod1", _ => { }, null, new Attribute[]
                                                                                {
                                                                                    new FactAttribute(),
                                                                                    new CustomBeforeAfterTestAttribute(exception)
                                                                                });

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

        private class CustomBeforeAfterTestAttribute : BeforeAfterTestAttribute
        {
            private readonly Exception exception;

            public CustomBeforeAfterTestAttribute(Exception exception)
            {
                this.exception = exception;
            }

            public override void Before(MethodInfo methodUnderTest)
            {
                throw exception;
            }
        }
    }
}