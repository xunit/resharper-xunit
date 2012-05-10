using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_a_fact_fails : SingleClassTestRunContext
    {
        [Fact]
        public void Should_notify_method_exception()
        {
            var exception = new SingleException(33);
            var method = testClass.AddFailingTest("TestMethod1", exception);

            Run();

            Messages.AssertContainsTaskException(method.Task, exception);
        }

        [Fact]
        public void Should_notify_method_finished_with_errors()
        {
            var exception = new SingleException(23);
            var method = testClass.AddFailingTest("TestMethod1", exception);

            Run();

            Messages.AssertContainsTaskFinished(method.Task, exception.UserMessage, TaskResult.Exception);
        }

        [Fact]
        public void Should_notify_exception_before_method_finished()
        {
            var exception = new SingleException(23);
            var method = testClass.AddFailingTest("TestMethod1", exception);

            Run();

            var messages = Messages.AssertContainsTaskException(method.Task, exception);
            messages.AssertContainsTaskFinished(method.Task, exception.UserMessage, TaskResult.Exception);
        }
    }
}