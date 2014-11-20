using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    public class When_capturing_output_from_theory : XunitTaskRunnerOutputTestBase
    {
        private const string TypeName = "Foo.CapturesOutputFromTheory";

        public When_capturing_output_from_theory(string environment)
            : base(environment)
        {
        }

        protected override string GetTestName()
        {
            return "CapturesOutputFromTheory." + XunitEnvironment;
        }

        [Test]
        public void Should_notify_output_for_successful_test()
        {
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputFromSuccessfulTest", "OutputFromSuccessfulTest(value: 42)"),
                "This is some output");
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputFromSuccessfulTest", "OutputFromSuccessfulTest(value: 11)"),
                "This is some output");
        }

        [Test]
        public void Should_notify_output_for_failing_test()
        {
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputFromFailingTest", "OutputFromFailingTest(value: 42)"),
                "This is also some output");
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputFromFailingTest", "OutputFromFailingTest(value: 12)"),
                "This is also some output");
        }

        [Test]
        public void Should_notify_output_between_start_and_end_of_method()
        {
            AssertMessageOrder(ForTaskAndChildren(TypeName, "OutputFromSuccessfulTest"),
                TaskAction.Start,       // Method
                    TaskAction.Start,   // Theory
                        TaskAction.Output,
                        TaskAction.Duration,
                    TaskAction.Finish,
                    TaskAction.Start,
                        TaskAction.Output,
                        TaskAction.Duration,
                    TaskAction.Finish,  //  Theory
                TaskAction.Finish);     // Method
        }
    }
}