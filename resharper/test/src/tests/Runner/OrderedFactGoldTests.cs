namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class OrderedFactGoldTests : XunitTaskRunnerTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return base.RelativeTestDataPath + @"Gold\"; }
        }

        [XunitEnvironmentTests]
        public void TestPassingFact(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "PassingFact");
        }

        [XunitEnvironmentTests]
        public void TestFailingFact(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "FailingFact");
        }

        [XunitEnvironmentTests]
        public void TestSkippedFact(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "SkippedFact");
        }

        [XunitEnvironmentTests]
        public void TestFactWithInvalidParameters(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "FactWithInvalidParameters");
        }

        [XunitEnvironmentTests]
        public void TestCustomFactAttributeSkips(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "CustomFactAttributeSkips");
        }

        [XunitEnvironmentTests]
        public void TestAmbiguouslyNamedTestMethods(IXunitEnvironment environment)
        {
            // TODO: This misses a test to continue running next class. Ordering.
            DoOneTestWithStrictOrdering(environment, "AmbiguouslyNamedTestMethods");
        }
    }
}