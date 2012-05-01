using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_running_a_single_successful_test_method : SingleClassTestRunContext
    {
        [Fact]
        public void Should_notify_method_starting()
        {
            var method = testClass.AddPassingTest("TestMethod1");
            Run();

            Messages.AssertContainsTaskStarting(method.Task);
        }

        [Fact]
        public void Should_notify_method_finished_successfully()
        {
            var method = testClass.AddPassingTest("TestMethod1");
            Run();

            Messages.AssertContainsTaskFinished(method.Task, string.Empty, TaskResult.Success);
        }

        [Fact]
        public void Should_notify_method_finished_after_method_start()
        {
            var method = testClass.AddPassingTest("TestMethod1");
            Run();

            var messages = Messages.AssertContainsTaskStarting(method.Task);
            messages.AssertContainsTaskFinished(method.Task, string.Empty, TaskResult.Success);
        }
    }
}