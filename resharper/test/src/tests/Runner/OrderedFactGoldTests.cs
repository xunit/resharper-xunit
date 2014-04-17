using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    [TestFixture("xunit1")]
    [TestFixture("xunit2")]
    public class OrderedFactGoldTests : XunitTaskRunnerTestBase
    {
        public OrderedFactGoldTests(string environmentId)
            : base(environmentId)
        {
        }

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

        [Test]
        public void TestAmbiguouslyNamedTestMethods()
        {
            // TODO: This misses a test to continue running next class. Ordering.
            DoOneTestWithStrictOrdering("AmbiguouslyNamedTestMethods");
        }
    }
}