using System.Threading;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitMetdataTestBase
    {
        protected override void ExploreAssembly(IProject testProject, IMetadataAssembly metadataAssembly, UnitTestElementConsumer add)
        {
            GetMetdataExplorer().ExploreAssembly(testProject, metadataAssembly, add, new ManualResetEvent(false));
        }
    }
}