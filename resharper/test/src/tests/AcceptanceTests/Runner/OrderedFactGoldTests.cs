using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
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
        public void TestInStaticClass()
        {
            DoOneTestWithStrictOrdering("StaticClass");
        }

        [Test]
        public void TestFactWithInvalidParameters()
        {
            DoOneTestWithStrictOrdering("FactWithInvalidParameters");
        }

        [Test]
        public void TestAmbiguouslyNamedTestMethods()
        {
            // TODO: This misses a test to continue running next class.
            // I can't remember what that means :(
            DoOneTestWithStrictOrdering("AmbiguouslyNamedTestMethods." + XunitEnvironment.Id);
        }

        [Test]
        public void TestEscapedStringsInDataAttributes()
        {
            DoOneTestWithStrictOrdering("EscapedDataAttributeStrings");
        }

        [Test]
        public void TestUnicodeStringsInDataAttributes()
        {
            DoOneTestWithStrictOrdering("UnicodeDataAttributeStrings");
        }

        [Test]
        public void TestDisplayName()
        {
            DoOneTestWithStrictOrdering("DisplayName");
        }

        [Test]
        public void TestUnicodeDisplayName()
        {
            DoOneTestWithStrictOrdering("UnicodeDisplayName");
        }
    }
}