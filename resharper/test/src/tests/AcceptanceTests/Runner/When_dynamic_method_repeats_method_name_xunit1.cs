using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [Category("xunit1")]
    public class When_dynamic_method_repeats_method_name_xunit1 : XunitTaskRunnerOutputTestBase
    {
        // TODO: Can we get into this situation in xunit2?
        public When_dynamic_method_repeats_method_name_xunit1()
            : base("xunit1")
        {
        }

        protected override string GetTestName()
        {
            return "DynamicMethodWithRepeatedName." + XunitEnvironment.Id;
        }

        private TaskId GetDuplicateMethodId(string suffix = null)
        {
            return ForTaskOnly("Foo.MethodWithRepeatedNames", "DoTests" + suffix);
        }

        [Test]
        public void Should_not_report_repeated_method_names_more_than_once()
        {
            AssertContainsStart(GetDuplicateMethodId());
        }

        [Test]
        public void Should_rename_second_usage_of_repeated_method_name()
        {
            AssertContainsStart(GetDuplicateMethodId());
            AssertContainsStart(GetDuplicateMethodId(" [2]"));
        }

        [Test]
        public void Should_rename_subsequent_usages_of_repeated_method_name()
        {
            AssertContainsStart(GetDuplicateMethodId());
            AssertContainsStart(GetDuplicateMethodId(" [2]"));
            AssertContainsStart(GetDuplicateMethodId(" [3]"));
        }
    }
}