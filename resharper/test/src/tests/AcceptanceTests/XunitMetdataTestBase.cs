using System;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ReSharper.FeaturesTestFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    [UseSystemsFrameworks]
    public abstract class XunitMetdataTestBase : UnitTestMetadataTestBase
    {
        protected override void ExploreAssembly(IProject testProject, IMetadataAssembly metadataAssembly, IUnitTestElementsObserver add)
        {
            GetMetdataExplorer().ExploreAssembly(testProject, metadataAssembly, add);
        }

        private XunitTestMetadataExplorer GetMetdataExplorer()
        {
            return new XunitTestMetadataExplorer(Solution.GetComponent<XunitTestProvider>(),
                Solution.GetComponent<UnitTestElementFactory>());
        }

        protected override void PrepareBeforeRun(IProject testProject)
        {
            foreach (var assembly in GetReferencedAssemblies())
            {
                var location = FileSystemPath.Parse(Environment.ExpandEnvironmentVariables(assembly));
                var reference = ProjectToAssemblyReference.CreateFromLocation(testProject, location);
                ((ProjectImpl) testProject).DoAddReference(reference);

                if (location.IsAbsolute && location.ExistsFile)
                {
                    var localCopy = TestDataPath2.Combine(location.Name);
                    if (!localCopy.ExistsFile || localCopy.FileModificationTimeUtc < location.FileModificationTimeUtc)
                        location.CopyFile(localCopy, true);
                }
            }
        }
    }
}