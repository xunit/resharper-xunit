using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class OrderedFactGoldTests : XunitTaskRunnerTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return base.RelativeTestDataPath + @"Gold\"; }
        }

        [Test]
        public void TestPassingFact()
        {
            DoOneTestWithStrictOrdering("PassingFact");
        }

        [Test]
        public void TestFailingFact()
        {
            DoOneTestWithStrictOrdering("FailingFact");
        }

        [Test]
        public void TestSkippedFact()
        {
            DoOneTestWithStrictOrdering("SkippedFact");
        }

        [Test]
        public void TestFactWithInvalidParameters()
        {
            DoOneTestWithStrictOrdering("FactWithInvalidParameters");
        }

        [Test]
        public void TestCustomFactAttributeSkips()
        {
            DoOneTestWithStrictOrdering("CustomFactAttributeSkips");
        }
    }
}