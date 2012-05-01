using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    // Calls TestStart multiple times with the same type + method, but the name is the fully qualified name of the method with method parameters
    public class When_running_theories_from_a_single_method : SingleClassTestRunContext
    {
        [Fact]
        public void Should_call_task_starting_once_for_method()
        {
            var method = testClass.AddMethod("TestMethod1");
            method.AddPassingTheoryTest("TestMethod1(12)");
            method.AddPassingTheoryTest("TestMethod1(33)");

            Run();

            var messages = Messages.AssertContainsTaskStarting(method.Task);
            messages.AssertDoesNotContain(TaskMessage.TaskStarting(method.Task));
        }

        [Fact]
        public void Should_call_task_finished_once_for_method()
        {
            var method = testClass.AddMethod("TestMethod1");
            method.AddPassingTheoryTest("TestMethod1(12)");
            method.AddPassingTheoryTest("TestMethod1(33)");

            Run();

            var messages = Messages.AssertContainsTaskFinished(method.Task, string.Empty, TaskResult.Success);
            messages.AssertDoesNotContain(TaskMessage.TaskFinished(method.Task, string.Empty, TaskResult.Success));
        }

        [Fact]
        public void Should_call_task_failed_on_the_method_if_any_theories_fail()
        {
            const string expectedMessage = "Broken1";
            var method = testClass.AddMethod("TestMethod1");
            method.AddPassingTheoryTest("TestMethod1(12)");
            method.AddFailingTheoryTest("TestMethod1(12)", new AssertException(expectedMessage));
            method.AddPassingTheoryTest("TestMethod1(12)");

            Run();

            Messages.AssertContainsTaskFinished(method.Task, expectedMessage, TaskResult.Exception);
        }

        [Fact]
        public void Should_display_last_reported_exception_on_task_finish()
        {
            const string expectedMessage = "Broken2";
            var method = testClass.AddMethod("TestMehod1");
            method.AddPassingTheoryTest("TestMethod1(12)");
            method.AddFailingTheoryTest("TestMethod1(22)", new AssertException("Broken1"));
            method.AddFailingTheoryTest("TestMethod1(33)", new AssertException(expectedMessage));
            method.AddPassingTheoryTest("TestMethod1(55)");

            Run();

            Messages.AssertContainsTaskFinished(method.Task, expectedMessage, TaskResult.Exception);
        }

        [Fact]
        public void Should_call_task_output_for_each_theory()
        {
            var method = testClass.AddMethod("TestMethod1");
            method.AddPassingTheoryTest("TestMethod1(12)", "output1");
            method.AddPassingTheoryTest("TestMethod1(33)", "output2");

            Run();

            Messages.AssertContainsTaskOutput(method.Task, "output1");
            Messages.AssertContainsTaskOutput(method.Task, "output2");
        }

        [Fact]
        public void Should_call_task_exception_for_each_failing_theory()
        {
            var method = testClass.AddMethod("TestMethod1");
            var exception1 = new AssertException("Broken1");
            var exception2 = new AssertException("Broken2");
            method.AddFailingTheoryTest("TestMethod1(12)", exception1);
            method.AddFailingTheoryTest("TestMethod1(33)", exception2);

            Run();

            Messages.AssertContainsTaskException(method.Task, exception1);
            Messages.AssertContainsTaskException(method.Task, exception2);
        }
    }
}