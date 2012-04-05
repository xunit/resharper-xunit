using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_running_a_single_successful_test_method
    {
        private readonly TestClassRun testClass;
        private readonly FakeRemoteTaskServer taskServer;

        public When_running_a_single_successful_test_method()
        {
            testClass = new TestClassRun("TestsNamespace.TestClass");
            taskServer = new FakeRemoteTaskServer();
        }

        private ReSharperRunnerLogger CreateLogger()
        {
            return new ReSharperRunnerLogger(taskServer, testClass.ClassTask, testClass.MethodTasks);
        }

        [Fact]
        public void Should_notify_method_starting()
        {
            var method = testClass.AddPassingTest("TestMethod1");
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskStarting(method.Task);
        }

        [Fact]
        public void Should_notify_method_finished()
        {
            var method = testClass.AddPassingTest("TestMethod1");
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskFinished(method.Task, string.Empty, TaskResult.Success);
        }

        [Fact]
        public void Should_notify_method_finished_after_method_start()
        {
            var method = testClass.AddPassingTest("TestMethod1");
            var logger = CreateLogger();
            testClass.Run(logger);

            var messages = taskServer.Messages.AssertContainsTaskStarting(method.Task);
            messages.AssertContainsTaskFinished(method.Task, string.Empty, TaskResult.Success);
        }
    }
}