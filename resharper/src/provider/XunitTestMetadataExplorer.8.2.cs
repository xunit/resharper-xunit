using System.Threading;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [MetadataUnitTestExplorer]
    public partial class XunitTestMetadataExplorer : IUnitTestMetadataExplorer
    {
        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, UnitTestElementConsumer consumer,
                                    ManualResetEvent exitEvent)
        {
            ExploreAssembly(project, assembly, new UnitTestElementsObserver(consumer));
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }
    }
}