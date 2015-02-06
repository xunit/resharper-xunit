using JetBrains.ReSharper.FeaturesTestFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestSupportTests;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    //[UseSystemsFrameworks]
    public abstract partial class XunitSourceTestBase : UnitTestSourceTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            EnvironmentVariables.SetUp(BaseTestDataPath);
        }
    }
}