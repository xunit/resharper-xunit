using System;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_all_facts_in_a_class_fail : SingleClassTestRunContext
    {
        [Fact]
        public void Should_still_notify_class_as_completed_successfully()
        {
            testClass.AddFailingTest("TestMethod1", new InvalidOperationException("Broken1"));
            testClass.AddFailingTest("TestMethod2", new InvalidOperationException("Broken2"));

            Run();

            Messages.OfSameTask(testClass.ClassTask).TaskFinished();
        }
    }
}