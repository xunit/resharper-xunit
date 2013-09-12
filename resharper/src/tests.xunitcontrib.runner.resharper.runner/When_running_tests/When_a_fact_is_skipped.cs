using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public partial class When_a_fact_is_skipped : SingleClassTestRunContext
    {
        [Fact]
        public void Should_notify_test_started()
        {
            var method = testClass.AddSkippedTest("TestMethod1", "Skipped reason");

            Run();

            Messages.OfTask(method.Task).TaskStarting();
        }

        [Fact]
        public void Should_notify_test_finished_with_skip_reason()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);

            Run();

            Messages.OfTask(method.Task).TaskFinished(expectedReason, TaskResult.Skipped);
        }
    }
}