using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class When_capturing_output_from_theory : XunitTaskRunnerOutputTestBase
    {
        private const string TypeName = "Foo.CapturesOutputFromTheory";

        protected override string GetTestName()
        {
            return "CapturesOutputFromTheory";
        }

        [Test]
        public void Should_notify_output_for_successful_test()
        {
            AssertContainsOutput(ForTask(TypeName, "OutputFromSuccessfulTest", "OutputFromSuccessfulTest(value: 42)"),
                "This is some output");
            AssertContainsOutput(ForTask(TypeName, "OutputFromSuccessfulTest", "OutputFromSuccessfulTest(value: 11)"),
                "This is some output");
        }

        [Test]
        public void Should_notify_output_for_failing_test()
        {
            AssertContainsOutput(ForTask(TypeName, "OutputFromFailingTest", "OutputFromFailingTest(value: 42)"),
                "This is also some output");
            AssertContainsOutput(ForTask(TypeName, "OutputFromFailingTest", "OutputFromFailingTest(value: 12)"),
                "This is also some output");
        }

        [Test]
        public void Should_notify_output_between_start_and_end_of_method()
        {
            AssertMessageOrder(ForTask(TypeName, "OutputFromSuccessfulTest"),
                "task-start",           // Class
                    "task-start",       // Method
                        "task-start",   // Theory
                            "task-output",
                            "task-duration",
                        "task-finish",
                        "task-start",
                            "task-output",
                            "task-duration",
                        "task-finish",  // Theory
                    "task-finish",      // Method
                "task-finish");         // Class
        }
    }
}