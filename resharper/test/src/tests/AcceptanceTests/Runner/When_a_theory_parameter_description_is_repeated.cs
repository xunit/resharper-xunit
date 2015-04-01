using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    public abstract class When_a_theory_parameter_description_is_repeated : XunitTaskRunnerOutputTestBase
    {
        protected When_a_theory_parameter_description_is_repeated(string environment)
            : base(environment)
        {
        }

        protected override string GetTestName()
        {
            // xunit2 uses [MemberData] instead of [PropertyData]
            return "TheoriesWithRepeatedParameterDescriptions." + XunitEnvironment.Id;
        }

        [Test]
        public void Should_not_report_repeated_theory_names_more_than_once()
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

        protected abstract TaskId GetDuplicateToStringValueTaskId(string suffix = null);

        private TaskId GetDuplicateConstantValueTaskId(string suffix = null)
        {
            return ForTaskOnly("Foo.TheoryWithConstantValue", "DuplicateConstantValue",
                "DuplicateConstantValue(value: 12)" + suffix);
        }
    }

    [Category("xunit1")]
    public class When_a_theory_parameter_description_is_repeated_xunit1 :
        When_a_theory_parameter_description_is_repeated
    {
        public When_a_theory_parameter_description_is_repeated_xunit1()
            : base("xunit1")
        {
        }

        protected override TaskId GetDuplicateToStringValueTaskId(string suffix = null)
        {
            return ForTaskOnly("Foo.TheoryWithToStringValue", "DuplicateToStringValue",
                "DuplicateToStringValue(data: AlwaysTheSame.Data)" + suffix);
        }
    }

    [Category("xunit2")]
    public class When_a_theory_parameter_description_is_repeated_xunit2 :
        When_a_theory_parameter_description_is_repeated
    {
        public When_a_theory_parameter_description_is_repeated_xunit2()
            : base("xunit2")
        {
        }

        protected override TaskId GetDuplicateToStringValueTaskId(string suffix = null)
        {
            return ForTaskOnly("Foo.TheoryWithToStringValue", "DuplicateToStringValue",
                "DuplicateToStringValue(value: AlwaysTheSame.Data)" + suffix);
        }
    }
}