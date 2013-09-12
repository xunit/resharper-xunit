using System;
using System.Reflection;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_custom_before_after_attribute_throws : SingleClassTestRunContext
    {
        private readonly InvalidOperationException exception;
        private readonly Method method;

        public When_custom_before_after_attribute_throws()
        {
            // If a BeforeAfterAttribute throws, it's caught and rethrown by BeforeAfterCommand and then
            // caught by ExceptionAndOutputCaptureCommand, which returns a FailedResult. So this is a normal
            // failing test method
            exception = new InvalidOperationException("Thrown by a custom before/after attribute");
            method = testClass.AddMethod("TestMethod1", _ => { }, null, new Attribute[]
                {
                    new FactAttribute(),
                    new CustomBeforeAfterTestAttribute(exception)
                });
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
        public void Should_report_failed_test_finished()
        {
            Run();

            Messages.OfTask(method.Task).AssertTaskFinishedBadly(exception);
        }

        [Fact]
        public void Should_continue_running_tests()
        {
            var method2 = testClass.AddMethod("TestMethod2", _ => { }, null, new Attribute[]
                {
                    new FactAttribute(),
                    new CustomBeforeAfterTestAttribute(exception)
                });

            Run();

            Messages.OfTask(method.Task).AssertOrderedActions(ServerAction.TaskStarting, ServerAction.TaskException, ServerAction.TaskFinished);
            Messages.OfTask(method2.Task).AssertOrderedActions(ServerAction.TaskStarting, ServerAction.TaskException, ServerAction.TaskFinished);
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