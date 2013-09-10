using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_class_contains_multiple_facts : SingleClassTestRunContext
    {
        [Fact]
        public void Should_notify_class_starting_just_once()
        {
            testClass.AddPassingTest("TestMethod1");

            Run();

            Messages.OfSameTask(testClass.ClassTask).TaskStarting();
        }

        [Fact]
        public void Should_notify_class_finished_just_once()
        {
            testClass.AddPassingTest("TestMethod1");

            Run();

            Messages.OfSameTask(testClass.ClassTask).TaskFinished();
        }

        [Fact]
        public void Should_notify_class_before_and_after_all_tests()
        {
            var method = testClass.AddPassingTest("TestMethod1");
            Run();

            Messages.AssertOrderedSubsetWithSameTasks(new[]
                {
                    TaskMessage.TaskStarting(testClass.ClassTask),
                    TaskMessage.TaskStarting(method.Task),
                    TaskMessage.TaskFinished(method.Task, string.Empty, TaskResult.Success),
                    TaskMessage.TaskFinished(testClass.ClassTask, string.Empty, TaskResult.Success)
                });
        }

        [Fact]
        public void Should_notify_test_starting_for_each_test_method()
        {
            var method1 = testClass.AddPassingTest("TestMethod1");
            var method2 = testClass.AddPassingTest("TestMethod2");

            Run();

            Messages.OfSameTask(method1.Task).TaskStarting();
            Messages.OfSameTask(method2.Task).TaskStarting();
        }

        [Fact]
        public void Should_notify_test_finished_for_each_test_method()
        {
            var method1 = testClass.AddPassingTest("TestMethod1");
            var method2 = testClass.AddPassingTest("TestMethod2");

            Run();

            Messages.OfSameTask(method1.Task).TaskFinished();
            Messages.OfSameTask(method2.Task).TaskFinished();
        }

        [Fact]
        public void Should_notify_output_for_each_test_method()
        {
            const string expectedOutput1 = "This is some output";
            const string expectedOutput2 = "This is some output for method2";
            var method1 = testClass.AddPassingTest("TestMethod1", expectedOutput1);
            var method2 = testClass.AddPassingTest("TestMethod2", expectedOutput2);

            Run();

            Messages.OfSameTask(method1.Task).TaskOutput(expectedOutput1);
            Messages.OfSameTask(method2.Task).TaskOutput(expectedOutput2);
        }

        [Fact]
        public void Should_notify_test_finished_before_starting_next_test()
        {
            var method1 = testClass.AddPassingTest("TestMethod1");
            var method2 = testClass.AddPassingTest("TestMethod2");

            Run();

            Messages.AssertOrderedSubsetWithSameTasks(new[]
                {
                    TaskMessage.TaskFinished(method1.Task, string.Empty, TaskResult.Success),
                    TaskMessage.TaskStarting(method2.Task)
                });
        }

        [Fact]
        public void Should_continue_running_tests_after_failing_test_method()
        {
            var exception = new SingleException(33);
            var method1 = testClass.AddFailingTest("TestMethod1", exception);
            var method2 = testClass.AddPassingTest("TestMethod2");

            Run();

            Messages.AssertOrderedSubsetWithSameTasks(new[]
                {
                    TaskMessage.TaskException(method1.Task, exception),
                    TaskMessage.TaskStarting(method2.Task)
                });
        }
    }
}