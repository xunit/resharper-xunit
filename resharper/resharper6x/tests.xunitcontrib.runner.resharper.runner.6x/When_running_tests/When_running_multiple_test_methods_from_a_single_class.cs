using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_running_multiple_test_methods_from_a_single_class : TestRunContext
    {
        [Fact]
        public void Should_notify_class_starting_just_once()
        {
            testClass.AddPassingTest("TestMethod1");
            var logger = CreateLogger();
            testClass.Run(logger);

            var expectedTaskMessage = TaskMessage.TaskStarting(testClass.ClassTask);
            var messages = taskServer.Messages.AssertContains(expectedTaskMessage);
            messages.AssertDoesNotContain(expectedTaskMessage);
        }

        [Fact]
        public void Should_notify_class_finished_just_once()
        {
            testClass.AddPassingTest("TestMethod1");
            var logger = CreateLogger();
            testClass.Run(logger);

            var expectedTaskMessage = TaskMessage.TaskFinished(testClass.ClassTask, string.Empty, TaskResult.Success);
            var messages = taskServer.Messages.AssertContains(expectedTaskMessage);
            messages.AssertDoesNotContain(expectedTaskMessage);
        }

        [Fact]
        public void Should_notify_class_before_and_after_all_tests()
        {
            var method = testClass.AddPassingTest("TestMethod1");
            var logger = CreateLogger();
            testClass.Run(logger);

            var messages = taskServer.Messages.AssertContainsTaskStarting(testClass.ClassTask);
            messages = messages.AssertContainsTaskStarting(method.Task);
            messages.AssertContainsTaskFinished(testClass.ClassTask, string.Empty, TaskResult.Success);
        }

        [Fact]
        public void Should_notify_test_starting_for_each_test_method()
        {
            var method1 = testClass.AddPassingTest("TestMethod1");
            var method2 = testClass.AddPassingTest("TestMethod2");
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskStarting(method1.Task);
            taskServer.Messages.AssertContainsTaskStarting(method2.Task);
        }

        [Fact]
        public void Should_notify_test_finished_for_each_test_method()
        {
            var method1 = testClass.AddPassingTest("TestMethod1");
            var method2 = testClass.AddPassingTest("TestMethod2");
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskFinished(method1.Task, string.Empty, TaskResult.Success);
            taskServer.Messages.AssertContainsTaskFinished(method2.Task, string.Empty, TaskResult.Success);
        }

        [Fact]
        public void Should_notify_output_for_each_test_method()
        {
            const string expectedOutput1 = "This is some output";
            const string expectedOutput2 = "This is some output for method2";
            var method1 = testClass.AddPassingTest("TestMethod1", expectedOutput1);
            var method2 = testClass.AddPassingTest("TestMethod2", expectedOutput2);
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskOutput(method1.Task, expectedOutput1);
            taskServer.Messages.AssertContainsTaskOutput(method2.Task, expectedOutput2);
        }

        [Fact]
        public void Should_notify_test_finished_before_starting_next_test()
        {
            var method1 = testClass.AddPassingTest("TestMethod1");
            var method2 = testClass.AddPassingTest("TestMethod2");
            var logger = CreateLogger();
            testClass.Run(logger);

            var messages = taskServer.Messages.AssertContainsTaskStarting(method1.Task);
            messages.AssertContainsTaskFinished(method2.Task, string.Empty, TaskResult.Success);
        }

        [Fact]
        public void Should_continue_running_tests_after_failing_test_method()
        {
            var exception = new SingleException(33);
            var method1 = testClass.AddFailingTest("TestMethod1", exception);
            var method2 = testClass.AddPassingTest("TestMethod2");
            var logger = CreateLogger();
            testClass.Run(logger);

            var messages = taskServer.Messages.AssertContainsTaskException(method1.Task, exception);
            messages.AssertContainsTaskStarting(method2.Task);
        }
    }
}