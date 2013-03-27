using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public partial class When_a_fact_is_skipped
    {
        // TaskExplain doesn't exist in 8.0
        [Fact]
        public void Should_notify_test_skipped_with_reason()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);

            Run();

            Messages.AssertSameTask(method.Task).TaskExplain(expectedReason);
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