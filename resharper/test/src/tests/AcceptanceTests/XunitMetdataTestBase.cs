using System;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Impl;
using JetBrains.ReSharper.FeaturesTestFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestSupportTests;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

// ReSharper 9.0 doesn't define this, used by 8.2
namespace JetBrains.ReSharper.UnitTestSupportTests
{
}

// ReSharper 8.2 doesn't define this, used by 9.0
namespace JetBrains.ReSharper.FeaturesTestFramework.UnitTesting
{
}

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitMetdataTestBase : UnitTestMetadataTestBase
    {
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