using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Runner
{
    [TestFixture("xunit1", Category = "xunit1")]
    [TestFixture("xunit2", Category = "xunit2")]
    public class TestsInStaticClasses : XunitTaskRunnerTestBase
    {
        public TestsInStaticClasses(string environmentId)
            : base(environmentId)
        {
        }
    }
}