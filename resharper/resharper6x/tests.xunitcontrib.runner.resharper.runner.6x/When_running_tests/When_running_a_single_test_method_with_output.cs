using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_running_a_single_test_method_with_output
    {
        private readonly TestClassRun testClass;
        private readonly FakeRemoteTaskServer taskServer;

        public When_running_a_single_test_method_with_output()
        {
            testClass = new TestClassRun("TestsNamespace.TestClass");
            taskServer = new FakeRemoteTaskServer();
        }

        private ReSharperRunnerLogger CreateLogger()
        {
            return new ReSharperRunnerLogger(taskServer, testClass.ClassTask, testClass.MethodTasks);
        }

        [Fact]
        public void Should_notify_output_for_successful_test()
        {
            const string expectedOutput = "This is some output";
            var method = testClass.AddPassingTest("TestMethod1", expectedOutput);
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskOutput(method.Task, expectedOutput);
        }

        [Fact]
        public void Should_notify_output_for_failing_test()
        {
            const string expectedOutput = "This is some output";
            var exception = new SingleException(33);
            var method = testClass.AddFailingTest("TestMethod1", exception, expectedOutput);
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskOutput(method.Task, expectedOutput);
        }

        [Fact]
        public void Should_notify_output_between_start_and_end_of_method()
        {
            const string expectedOutput = "This is some output";
            var method = testClass.AddPassingTest("TestMethod1", expectedOutput);
            var logger = CreateLogger();
            testClass.Run(logger);

            var messages = taskServer.Messages.AssertContainsTaskStarting(method.Task);
            messages = messages.AssertContainsTaskOutput(method.Task, expectedOutput);
            messages.AssertContainsSuccessfulTaskFinished(method.Task);
        }
    }
}