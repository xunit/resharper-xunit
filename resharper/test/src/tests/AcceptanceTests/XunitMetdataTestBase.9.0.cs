using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitMetdataTestBase
    {
        protected override void ExploreAssembly(IProject testProject, IMetadataAssembly metadataAssembly, IUnitTestElementsObserver add)
        {
            GetMetdataExplorer().ExploreAssembly(testProject, metadataAssembly, add);
        }

        protected override string GetIdString(IUnitTestElement element)
        {
            return string.Format("{0}::{1}::{2}", element.Id.Provider.ID, element.Id.PersistentProjectId.Id, element.Id.Id);
        }
    }
}