using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public class TestDiscoveryVisitor : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        public TestDiscoveryVisitor()
        {
            TestCases = new List<ITestCase>();
        }

        public List<ITestCase> TestCases { get; private set; }

        protected override bool Visit(ITestCaseDiscoveryMessage discovery)
        {
            TestCases.Add(discovery.TestCase);
            return true;
        }

        public override void Dispose()
        {
            foreach (var testCase in TestCases)
                testCase.Dispose();
            base.Dispose();
        }
    }
}