using System.Linq;
using Xunit;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_class_contains_ambiguously_named_test_methods
    {
        private readonly TestRun testRun;
        private readonly Class testClass;

        public When_class_contains_ambiguously_named_test_methods()
        {
            testRun = TestRun.SingleClassRun;
            testClass = testRun.Classes.Single();

            testClass.AddPassingTest("TestMethod1");
            testClass.AddPassingTest("TestMethod1");
        }

        [Fact]
        public void Should_start_class()
        {
            testRun.Run();

            testRun.Messages.AssertContainsTaskStarting(testClass.ClassTask);
        }

        [Fact]
        public void Should_finish_class()
        {
            testRun.Run();

            testRun.Messages.AssertContainsFailedInfrastructureTaskFinished(testClass.ClassTask, testClass.Exception);
        }

        [Fact]
        public void Should_report_exception_as_failure_in_class()
        {
            testRun.Run();

            testRun.Messages.AssertContainsTaskException(testClass.ClassTask, testClass.Exception);
            testRun.Messages.AssertContainsFailedInfrastructureTaskFinished(testClass.ClassTask, testClass.Exception);
        }

        [Fact]
        public void Should_not_run_any_test_method_in_class()
        {
            var method = testClass.AddPassingTest("ValidTest");

            testRun.Run();

            Assert.False(testRun.Messages.Any(m => m.Task == method.Task), "Should not notify server for test method");
        }

        [Fact]
        public void Should_continue_running_next_class()
        {
            // This only runs again because we use xunit's RunTests method, and call it once for each test class
            // If xunit were running the whole assembly, it would stop the run at the first error
            var @class = testRun.AddClass("TestsNamespace.TestClass2");
            var method = @class.AddPassingTest("ValidTest");

            testRun.Run();

            testRun.Messages.AssertContainsSuccessfulTaskFinished(method.Task);
            testRun.Messages.AssertContainsSuccessfulTaskFinished(@class.ClassTask);
        }
    }
}