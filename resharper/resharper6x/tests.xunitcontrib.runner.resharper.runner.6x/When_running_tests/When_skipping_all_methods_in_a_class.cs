using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_skipping_all_methods_in_a_class : SingleClassTestRunContext
    {
        [Fact]
        public void Should_notify_class_as_completed_successfully()
        {
            testClass.AddSkippedTest("TestMethod1", "Skipped reason");
            testClass.AddSkippedTest("TestMethod2", "Skipped reason");

            Run();

            Messages.AssertContainsSuccessfulTaskFinished(testClass.ClassTask);
        }
    }
}