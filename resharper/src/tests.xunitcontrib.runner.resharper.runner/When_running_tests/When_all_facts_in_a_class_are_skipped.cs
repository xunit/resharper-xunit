using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_all_facts_in_a_class_are_skipped : SingleClassTestRunContext
    {
        [Fact]
        public void Should_still_notify_class_as_completed_successfully()
        {
            testClass.AddSkippedTest("TestMethod1", "Skipped reason");
            testClass.AddSkippedTest("TestMethod2", "Skipped reason");

            Run();

            Messages.OfSameTask(testClass.ClassTask).TaskFinishedSuccessfully();
        }
    }
}