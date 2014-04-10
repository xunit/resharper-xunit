using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class OrderedTheoryGoldTests : XunitTaskRunnerTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return base.RelativeTestDataPath + @"Gold\"; }
        }

        [Test]
        public void TestPassingTheory()
        {
            DoOneTestWithStrictOrdering("PassingTheory");
        }

        [Test]
        public void TestFailingTheory()
        {
            DoOneTestWithStrictOrdering("FailingTheory");
        }

        [Test]
        public void TestTheoryWithInvalidParameters()
        {
            DoOneTestWithStrictOrdering("TheoryWithInvalidParameters");
        }

        [Test]
        public void TestCustomTheoryAttributeSkips()
        {
            DoOneTestWithStrictOrdering("CustomTheoryAttributeSkips");
        }
    }
}