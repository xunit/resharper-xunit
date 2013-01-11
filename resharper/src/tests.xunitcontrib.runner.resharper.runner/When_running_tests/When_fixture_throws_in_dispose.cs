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

        private class ThrowsInDispose : IDisposable
        {
            public static Exception Exception;

            public void Dispose()
            {
                throw Exception;
            }
        }

        public When_fixture_throws_in_dispose()
        {
            testRun = new TestRun();
            testClass = testRun.AddClass("TestNamespace.TestClass1");
            testClass.AddFixture<ThrowsInDispose>();
            ThrowsInDispose.Exception = new InvalidOperationException("Ooops");
            testClass.AddPassingTest("TestMethod1");
            testClass.AddPassingTest("TestMethod2");
        }

        [Fact]
        public void Should_notify_class_start()
        {
            testRun.Run();

            testRun.Messages.AssertSameTask(testClass.ClassTask).TaskStarting();
        }

        [Fact]
        public void Should_notify_class_exception()
        {
            testRun.Run();

            testRun.Messages.AssertSameTask(testClass.ClassTask).TaskException(ThrowsInDispose.Exception);
        }

        [Fact]
        public void Should_notify_class_failed()
        {
            testRun.Run();

            testRun.Messages.AssertSameTask(testClass.ClassTask).TaskFinished(ThrowsInDispose.Exception);
        }

        [Fact]
        public void Should_notify_all_methods_as_finished_and_failed()
        {
            testRun.Run();

            var methodTasks = testClass.MethodTasks.ToList();

            var taskException = new TaskException(null, string.Format("Class failed in {0}", testClass.ClassTask.TypeName), null);
            testRun.Messages.AssertSameTask(methodTasks[0]).TaskException(taskException);
            testRun.Messages.AssertSameTask(methodTasks[1]).TaskException(taskException);
        }

        [Fact]
        public void Should_continue_running_other_classes()
        {
            var testClass2 = testRun.AddClass("TestNamespace.TestClass2");
            var testMethod = testClass2.AddPassingTest("OtherMethod");

            testRun.Run();

            testRun.Messages.AssertSameTask(testClass2.ClassTask).TaskStarting();
            testRun.Messages.AssertSameTask(testMethod.Task).OrderedActions(ServerAction.TaskStarting, ServerAction.TaskFinished);
            testRun.Messages.AssertSameTask(testClass2.ClassTask).TaskFinished();
        }
    }
}