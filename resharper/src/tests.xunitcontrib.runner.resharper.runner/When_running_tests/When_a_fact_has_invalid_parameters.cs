using System;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_a_fact_has_invalid_parameters : SingleClassTestRunContext
    {
        [Fact]
        public void Should_report_exception()
        {
            var exception = new InvalidOperationException("Fact method " + testClass.ClassTask.TypeName + "." + "TestMethod1" + " cannot have parameters");
            var method = testClass.AddTestWithInvalidParameters("TestMethod1");

            Run();

            Messages.OfSameTask(method.Task).TaskException(exception);
        }

        [Fact]
        public void Should_fail_the_test()
        {
            var exception = new InvalidOperationException("Fact method " + testClass.ClassTask.TypeName + "." + "TestMethod1" + " cannot have parameters");
            var method = testClass.AddTestWithInvalidParameters("TestMethod1");

            Run();

            Messages.OfSameTask(method.Task).TaskFinished(exception);
        }
    }
}