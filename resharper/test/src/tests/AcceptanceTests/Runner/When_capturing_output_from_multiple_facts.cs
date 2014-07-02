using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [Category("xunit1")]
    public class When_capturing_output_from_multiple_facts : XunitTaskRunnerOutputTestBase
    {
        public When_capturing_output_from_multiple_facts()
            : base("xunit1")
        {
        }

        protected override string GetTestName()
        {
            return "MultipleFacts";
        }

        private TaskId Method1TaskId
        {
            get { return ForTaskOnly("Foo.MultipleFacts", "TestMethod1"); }
        }

        private TaskId Method2TaskId
        {
            get { return ForTaskOnly("Foo.MultipleFacts", "TestMethod2"); }
        }

        [Test]
        public void Should_notify_test_output_for_each_test_method()
        {
            AssertContainsOutput(Method1TaskId, "Output from TestMethod1");
            AssertContainsOutput(Method2TaskId, "Output from TestMethod2");
        }
    }
}