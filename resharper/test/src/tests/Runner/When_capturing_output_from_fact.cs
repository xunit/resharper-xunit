using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    // xunit2 doesn't support capturing output, due to parallelisation
    [TestFixture("xunit1")]
    public class When_capturing_output_from_fact : XunitTaskRunnerOutputTestBase
    {
        private const string TypeName = "Foo.CapturesOutput";

        public When_capturing_output_from_fact(string environmentId)
            : base(environmentId)
        {
        }

        protected override string GetTestName()
        {
            return "CapturesOutput";
        }

        [Test]
        public void Should_notify_output_for_successful_test()
        {
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputFromSuccessfulTest"),
                "This is some output");
        }

        [Test]
        public void Should_notify_output_for_failing_test()
        {
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputFromFailingTest"),
                "This is also some output");
        }

        [Test]
        public void Should_notify_output_between_start_and_end_of_method()
        {
            AssertMessageOrder(ForTaskAndChildren(TypeName, "OutputFromSuccessfulTest"),
                TaskAction.Start,
                    TaskAction.Output,
                    TaskAction.Duration,
                TaskAction.Finish);
        }
    }
}