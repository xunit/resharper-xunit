using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    public abstract class When_capturing_output_from_fact : XunitTaskRunnerOutputTestBase
    {
        protected const string TypeName = "Foo.CapturesOutput";

        protected When_capturing_output_from_fact(string environment)
            : base(environment)
        {
        }

        protected override string GetTestName()
        {
            return "CapturesOutput." + XunitEnvironment;
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

        [Test]
        public void Should_notify_test_output_for_each_test_method()
        {
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputFromSuccessfulTest"), "This is some output");
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputFromFailingTest"), "This is also some output");
        }
    }

    [Category("xunit1")]
    public class When_capturing_output_from_fact_xunit1 : When_capturing_output_from_fact
    {
        public When_capturing_output_from_fact_xunit1()
            : base("xunit1")
        {
        }
    }

    [Category("xunit2")]
    public class When_capturing_output_from_fact_xunit2 : When_capturing_output_from_fact
    {
        public When_capturing_output_from_fact_xunit2()
            : base("xunit2")
        {
        }

        [Test]
        public void Should_notify_output_as_it_happens()
        {
            AssertContainsOutput(ForTaskAndChildren(TypeName, "OutputMultipleTimes"),
                "Line 1", "Line 2", "Line 3");
        }
    }
}