using System;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_running_a_test_method_with_invalid_parameters : SingleClassTestRunContext
    {
        [Fact]
        public void Should_fail_the_test()
        {
            // TODO: Different test approach?
            // This is a normal failing test. It feels a bit rubbish to explicitly create
            // failing results. Would be nice to get xunit to do that for us
            Exception exception;
            var method = testClass.AddTestWithInvalidParameters("TestMethod1", out exception);

            Run();

            Messages.AssertContainsTaskStarting(method.Task);
            Messages.AssertContainsTaskException(method.Task, exception);
            Messages.AssertContainsTaskFinished(method.Task, exception.GetType().FullName + ": " + exception.Message, TaskResult.Exception);
        }
    }
}