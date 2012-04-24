using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_running_a_single_failing_test_method : TestRunContext
    {
        [Fact]
        public void Should_notify_method_exception()
        {
            var exception = new SingleException(33);
            var method = testClass.AddFailingTest("TestMethod1", exception);
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskException(method.Task, exception);
        }

        [Fact]
        public void Should_notify_method_finished_with_errors()
        {
            var exception = new SingleException(23);
            var method = testClass.AddFailingTest("TestMethod1", exception);
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskFinished(method.Task, exception.UserMessage, TaskResult.Exception);
        }

        [Fact]
        public void Should_notify_exception_before_method_finished()
        {
            var exception = new SingleException(23);
            var method = testClass.AddFailingTest("TestMethod1", exception);
            var logger = CreateLogger();
            testClass.Run(logger);

            var messages = taskServer.Messages.AssertContainsTaskException(method.Task, exception);
            messages.AssertContainsTaskFinished(method.Task, exception.UserMessage, TaskResult.Exception);
        }
    }
}