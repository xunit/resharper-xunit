using System;
using System.Linq;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_class_contains_ambiguously_named_test_methods : TestRunContext
    {
        [Fact]
        public void Should_start_class()
        {
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskStarting(testClass.ClassTask);
        }

        [Fact]
        public void Should_finish_class()
        {
            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskFinished(testClass.ClassTask, string.Empty, TaskResult.Success);
        }

        [Fact]
        public void Should_report_exception()
        {
            var exception = new InvalidOperationException("This is the exception");

            testClass.Exception = exception;

            var logger = CreateLogger();
            testClass.Run(logger);

            taskServer.Messages.AssertContainsTaskException(testClass.ClassTask, exception);
            taskServer.Messages.AssertContainsTaskFinished(testClass.ClassTask, 
                string.Format("{0}: {1}", exception.GetType().Name, exception.Message), TaskResult.Exception);
        }

        [Fact]
        public void Should_not_run_any_test_methods()
        {
            var method = testClass.AddMethod("TestMethod1");

            var logger = CreateLogger();
            testClass.Run(logger);

            Assert.False(taskServer.Messages.Any(m => m.Task == method.Task), "Should not notify server for test method");
        }

        [Fact(Skip = "Not yet implemented")]
        public void Should_continue_running_next_class()
        {
            throw new NotImplementedException();
        }
    }
}