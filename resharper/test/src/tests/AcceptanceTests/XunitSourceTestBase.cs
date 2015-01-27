using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.FeaturesTestFramework.UnitTesting;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    [UseSystemsFrameworks]
    public abstract class XunitSourceTestBase : UnitTestSourceTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            EnvironmentVariables.SetUp(BaseTestDataPath);
        }

        protected override IUnitTestElementsSource FileExplorer
        {
            get
            {
                return new XunitTestElementsSource(new XunitTestProvider(),
                    Solution.GetComponent<UnitTestElementFactory>(),
                    Solution.GetComponent<SearchDomainFactory>(),
                    Solution.GetComponent<IShellLocks>());
            }
        }
    }
}