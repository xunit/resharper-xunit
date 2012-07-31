using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_a_fact_has_output : SingleClassTestRunContext
    {
        [Fact]
        public void Should_notify_output_for_successful_test()
        {
            const string expectedOutput = "This is some output";
            var method = testClass.AddPassingTest("TestMethod1", expectedOutput);
            Run();

            Messages.AssertContainsTaskOutput(method.Task, expectedOutput);
        }

        [Fact]
        public void Should_notify_output_for_failing_test()
        {
            const string expectedOutput = "This is some output";
            var exception = new SingleException(33);
            var method = testClass.AddFailingTest("TestMethod1", exception, expectedOutput);
            Run();

            Messages.AssertContainsTaskOutput(method.Task, expectedOutput);
        }

        [Fact]
        public void Should_notify_output_between_start_and_end_of_method()
        {
            const string expectedOutput = "This is some output";
            var method = testClass.AddPassingTest("TestMethod1", expectedOutput);
            Run();

            var messages = Messages.AssertContainsTaskStarting(method.Task);
            messages = messages.AssertContainsTaskOutput(method.Task, expectedOutput);
            messages.AssertContainsSuccessfulTaskFinished(method.Task);
        }
    }
}