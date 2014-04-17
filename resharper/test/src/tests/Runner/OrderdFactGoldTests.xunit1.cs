using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class OrderedFactGoldTestsXunit1 : XunitTaskRunnerTestBase
    {
        public OrderedFactGoldTestsXunit1()
            : base("xunit1")
        {
        }

        protected override string RelativeTestDataPath
        {
            get { return base.RelativeTestDataPath + @"Gold\"; }
        }

        [Test]
        public void TestCustomFactAttributeSkips()
        {
            DoOneTestWithStrictOrdering("CustomFactAttributeSkips");
        }
    }
}