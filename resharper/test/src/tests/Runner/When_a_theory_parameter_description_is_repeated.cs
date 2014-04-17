using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    [TestFixture("xunit1")]
    [TestFixture("xunit2")]
    public class When_a_theory_parameter_description_is_repeated : XunitTaskRunnerOutputTestBase
    {
        public When_a_theory_parameter_description_is_repeated(string environment)
            : base(environment)
        {
        }

        protected override string GetTestName()
        {
            return "TheoriesWithRepeatedParameterDescriptions." + XunitEnvironment.Id;
        }

        [Test]
        public void Should_not_report_repeated_theory_names_more_Than_once()
        {
            AssertContainsStart(GetDuplicateToStringValueTaskId());
        }

        [Test]
        public void Should_rename_second_usage_of_repeated_theory_name()
        {
            AssertContainsStart(GetDuplicateToStringValueTaskId());
            AssertContainsStart(GetDuplicateToStringValueTaskId(" [2]"));
        }

        [Test]
        public void Should_rename_subsequent_usages_of_repeated_theory_name()
        {
            AssertContainsStart(GetDuplicateToStringValueTaskId());
            AssertContainsStart(GetDuplicateToStringValueTaskId(" [2]"));
            AssertContainsStart(GetDuplicateToStringValueTaskId(" [3]"));
        }

        [Test]
        public void Should_rename_subsequent_usages_of_theories_with_the_same_parameter_value()
        {
            AssertContainsStart(GetDuplicateConstantValueTaskId());
            AssertContainsStart(GetDuplicateConstantValueTaskId(" [2]"));
            AssertContainsStart(GetDuplicateConstantValueTaskId(" [3]"));
            AssertContainsStart(GetDuplicateConstantValueTaskId(" [4]"));
        }

        private TaskId GetDuplicateToStringValueTaskId(string suffix = null)
        {
            return ForTaskOnly("Foo.TheoryWithToStringValue", "DuplicateToStringValue",
                "DuplicateToStringValue(data: AlwaysTheSame.Data)" + suffix);
        }

        private TaskId GetDuplicateConstantValueTaskId(string suffix = null)
        {
            return ForTaskOnly("Foo.TheoryWithConstantValue", "DuplicateConstantValue",
                "DuplicateConstantValue(value: 12)" + suffix);
        }
    }
}