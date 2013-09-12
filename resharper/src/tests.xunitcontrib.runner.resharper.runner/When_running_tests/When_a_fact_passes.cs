using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_a_fact_passes : SingleClassTestRunContext
    {
        [Fact]
        public void Should_notify_method_starting()
        {
            var method = testClass.AddPassingTest("TestMethod1");

            Run();

            Messages.OfTask(method.Task).TaskStarting();
        }

        [Fact]
        public void Should_notify_method_finished_successfully()
        {
            var method = testClass.AddPassingTest("TestMethod1");

            Run();

            Messages.OfTask(method.Task).TaskFinishedSuccessfully();
        }

        [Fact]
        public void Should_notify_method_finished_after_method_start()
        {
            var method = testClass.AddPassingTest("TestMethod1");

            Run();

            Messages.OfTask(method.Task).AssertOrderedActions(ServerAction.TaskStarting, ServerAction.TaskFinished);
        }
    }
}