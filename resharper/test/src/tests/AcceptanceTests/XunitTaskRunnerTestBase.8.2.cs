using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitTaskRunnerTestBase
    {
        protected override IUnitTestMetadataExplorer MetadataExplorer
        {
            get
            {
                return new XunitTestMetadataExplorer(Solution.GetComponent<XunitTestProvider>(),
                    Solution.GetComponent<UnitTestElementFactory>());
            }
        }
    }
}
