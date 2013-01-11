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

            Messages.AssertSameTask(method.Task).TaskStarting();
        }

        [Fact]
        public void Should_notify_test_skipped_with_reason()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);

            Run();

            Messages.AssertSameTask(method.Task).TaskExplain(expectedReason);
        }

        [Fact]
        public void Should_notify_test_finished()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);

            Run();

            Messages.AssertSameTask(method.Task).TaskFinished(expectedReason, TaskResult.Skipped);
        }

        [Fact]
        public void Should_notify_explanation_before_finishing()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);

            Run();

            Messages.AssertSameTask(method.Task).OrderedActions(ServerAction.TaskStarting, ServerAction.TaskExplain, ServerAction.TaskFinished);

        }
    }
}