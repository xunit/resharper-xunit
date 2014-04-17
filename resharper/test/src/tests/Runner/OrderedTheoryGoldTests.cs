namespace XunitContrib.Runner.ReSharper.Tests.Runner
{
    public class OrderedTheoryGoldTests : XunitTaskRunnerTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return base.RelativeTestDataPath + @"Gold\"; }
        }

        [XunitEnvironmentTests]
        public void TestPassingTheory(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "PassingTheory");
        }

        [XunitEnvironmentTests]
        public void TestFailingTheory(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "FailingTheory");
        }

        [XunitEnvironmentTests]
        public void TestTheoryWithInvalidParameters(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "TheoryWithInvalidParameters");
        }

        [XunitEnvironmentTests]
        public void TestCustomTheoryAttributeSkips(IXunitEnvironment environment)
        {
            DoOneTestWithStrictOrdering(environment, "CustomTheoryAttributeSkips");
        }
    }
}