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

            Messages.OfTask(method.Task).AssertTaskOutput(expectedOutput);
        }

        [Fact]
        public void Should_notify_output_for_failing_test()
        {
            const string expectedOutput = "This is some output";
            var exception = new SingleException(33);
            var method = testClass.AddFailingTest("TestMethod1", exception, expectedOutput);

            Run();

            Messages.OfTask(method.Task).AssertTaskOutput(expectedOutput);
        }

        [Fact]
        public void Should_notify_output_between_start_and_end_of_method()
        {
            const string expectedOutput = "This is some output";
            var method = testClass.AddPassingTest("TestMethod1", expectedOutput);

            Run();

            Messages.OfTask(method.Task).AssertOrderedActions(ServerAction.TaskStarting, ServerAction.TaskOutput, ServerAction.TaskFinished);
        }
    }
}