using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestSupportTests;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitSourceTestBase
    {
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