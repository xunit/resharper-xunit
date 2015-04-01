using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitMetdataTestBase
    {
        protected override void ExploreAssembly(IProject testProject, IMetadataAssembly metadataAssembly, UnitTestElementConsumer add)
        {
            GetMetdataExplorer().ExploreAssembly(testProject, metadataAssembly, new UnitTestElementsObserver(add));
        }
    }
}