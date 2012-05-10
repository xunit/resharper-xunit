using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_a_fact_is_skipped : SingleClassTestRunContext
    {
        [Fact]
        public void Should_notify_test_started()
        {
            var method = testClass.AddSkippedTest("TestMethod1", "Skipped reason");

            Run();

            Messages.AssertContainsTaskStarting(method.Task);
        }

        [Fact]
        public void Should_notify_test_skipped_with_reason()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);

            Run();

            Messages.AssertContainsTaskExplain(method.Task, expectedReason);
        }

        [Fact]
        public void Should_notify_test_finished()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);

            Run();

            Messages.AssertContainsTaskFinished(method.Task, expectedReason, TaskResult.Skipped);
        }
    }
}