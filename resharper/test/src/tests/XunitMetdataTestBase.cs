using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestSupportTests;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests
{
    public class XunitMetdataTestBase : UnitTestMetadataTestBase
    {
        protected override void ExploreAssembly(IProject testProject, IMetadataAssembly metadataAssembly, UnitTestElementConsumer add)
        {
            GetMetdataExplorer().ExploreAssembly(testProject, metadataAssembly, add);
        }

        private XunitTestMetadataExplorer GetMetdataExplorer()
        {
            return new XunitTestMetadataExplorer(Solution.GetComponent<XunitTestProvider>(),
                Solution.GetComponent<UnitTestElementFactory>(),
                Solution.GetComponent<UnitTestingAssemblyLoader>());
        }

        protected override void PrepareBeforeRun(IProject testProject)
        {
            foreach (var assembly in GetReferencedAssemblies())
            {
                var location = FileSystemPath.Parse(assembly);
                var reference = ProjectToAssemblyReference.CreateFromLocation(testProject, location);
                ((ProjectImpl) testProject).DoAddReference(reference);
            }
        }
    }
}