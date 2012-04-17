using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_skipping_a_single_test_method
    {
        private readonly TestClassRun testClass;
        private readonly FakeRemoteTaskServer taskServer;

        public When_skipping_a_single_test_method()
        {
            testClass = new TestClassRun("TestsNamespace.TestClass");
            taskServer = new FakeRemoteTaskServer();
        }

        private ReSharperRunnerLogger CreateLogger()
        {
            return new ReSharperRunnerLogger(taskServer, testClass.ClassTask, testClass.MethodTasks);
        }

        [Fact]
        public void Should_notify_test_started()
        {
            var method = testClass.AddSkippedTest("TestMethod1", "Skipped reason");
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskStarting(method.Task);
        }

        [Fact]
        public void Should_notify_test_skipped_with_reason()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskExplain(method.Task, expectedReason);
        }

        [Fact]
        public void Should_notify_test_finished()
        {
            const string expectedReason = "Skipped reason";
            var method = testClass.AddSkippedTest("TestMethod1", expectedReason);
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskFinished(method.Task, expectedReason, TaskResult.Skipped);
        }
    }
}