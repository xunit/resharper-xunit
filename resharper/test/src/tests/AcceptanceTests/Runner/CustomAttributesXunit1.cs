using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [Category("xunit1")]
    public class CustomAttributesXunit1 : XunitTaskRunnerTestBase
    {
        public CustomAttributesXunit1()
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

        [Test]
        public void TestCustomTheoryAttributeSkips()
        {
            DoOneTestWithStrictOrdering("CustomTheoryAttributeSkips");
        }
    }
}