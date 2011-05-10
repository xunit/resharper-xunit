using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.UnitTestRunnerProvider;
using XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.UnitTestRunnerElements;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [MetadataUnitTestExplorer]
    public class XunitTestMetadataExplorer : XunitRunnerMetadataExplorer, IUnitTestMetadataExplorer
    {
        private readonly XunitTestProvider provider;

        public XunitTestMetadataExplorer(XunitTestProvider provider) : base(provider)
        {
            this.provider = provider;
        }

        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, UnitTestElementConsumer consumer)
        {
            Project = project;
            ExploreAssembly(assembly, consumer);
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }

        private IProject Project { get; set; }

        protected override XunitTestClassElement GetOrCreateTestClassElement(IMetadataAssembly assembly, string typeName, string shortName)
        {
            return provider.GetOrCreateTestClass(typeName, Project, typeName, shortName, assembly.Location.FullPath);
        }

        protected override XunitTestMethodElement GetOrCreateTestMethodElement(XunitTestClassElement parentTestClassElement,
                                                                          string typeName, string methodName, bool isSkip)
        {
            return provider.GetOrCreateTestMethod(typeName + "." + methodName, Project, (XunitViewTestClassElement) parentTestClassElement, typeName, methodName, isSkip);
        }
    }
}