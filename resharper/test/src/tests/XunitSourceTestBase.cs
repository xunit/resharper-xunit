using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestSupportTests;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests
{
    public abstract class XunitSourceTestBase : UnitTestSourceTestBase
    {
        protected override IUnitTestFileExplorer FileExplorer
        {
            get
            {
                return new XunitTestFileExplorer(new XunitTestProvider(),
                    Solution.GetComponent<UnitTestProviders>(),
                    Solution.GetComponent<UnitTestElementFactory>(),
                    Solution.GetComponent<SearchDomainFactory>());
            }
        }
    }
}