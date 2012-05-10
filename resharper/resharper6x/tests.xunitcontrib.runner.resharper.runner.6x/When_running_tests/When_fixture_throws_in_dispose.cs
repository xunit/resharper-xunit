using System;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_fixture_throws_in_dispose
    {
        private readonly TestRun testRun;
        private readonly Class testClass;
        private readonly Exception classException;

        public When_fixture_throws_in_dispose()
        {
            testRun = new TestRun();
            testClass = testRun.AddClass("TestNamespace.TestClass1");
            // TODO: This simulates an exception thrown in the constructor. Looks exactly like throwing in constructor, doesn't it?
            classException = new InvalidOperationException("Ooops");
            testClass.ClassResult.SetException(classException);
            testClass.AddMethod("TestMethod1");
            testClass.AddMethod("TestMethod2");
        }

        [Fact]
        public void Should_notify_class_start()
        {
            testRun.Run();

            testRun.Messages.AssertContainsTaskStarting(testClass.ClassTask);
        }

        [Fact]
        public void Should_notify_class_exception()
        {
            testRun.Run();

            testRun.Messages.AssertContainsTaskException(testClass.ClassTask, classException);
        }

        [Fact]
        public void Should_notify_class_failed()
        {
            testRun.Run();

            testRun.Messages.AssertContainsFailedTaskFinished(testClass.ClassTask, classException);
        }

        [Fact]
        public void Should_notify_all_methods_as_finished_and_failed()
        {
            testRun.Run();

            var methodTasks = testClass.MethodTasks.ToList();

            var taskException = new TaskException(null, string.Format("Class failed in {0}", testClass.ClassTask.TypeName), null);
            testRun.Messages.AssertContains(TaskMessage.TaskException(methodTasks[0], new[] { taskException }));
            testRun.Messages.AssertContains(TaskMessage.TaskException(methodTasks[1], new[] { taskException }));
        }

        [Fact]
        public void Should_continue_running_other_classes()
        {
            var testClass2 = testRun.AddClass("TestNamespace.TestClass2");
            var testMethod = testClass2.AddPassingTest("OtherMethod");

            testRun.Run();

            testRun.Messages.AssertContainsTaskStarting(testClass2.ClassTask);
            testRun.Messages.AssertContainsTaskStarting(testMethod.Task);
            testRun.Messages.AssertContainsSuccessfulTaskFinished(testMethod.Task);
            testRun.Messages.AssertContainsSuccessfulTaskFinished(testClass2.ClassTask);
        }
    }
}