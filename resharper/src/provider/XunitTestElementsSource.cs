using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Application;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using JetBrains.Util.Logging;


namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class XunitTestElementsSource : IUnitTestElementsSource
    {
        private readonly XunitServiceProvider services;
        private readonly SearchDomainFactory searchDomainFactory;
        private readonly MetadataElementsSource metadataElementsSource;

        public XunitTestElementsSource(XunitServiceProvider services, SearchDomainFactory searchDomainFactory, IShellLocks shellLocks)
        {
            this.services = services;
            this.searchDomainFactory = searchDomainFactory;

            metadataElementsSource = new MetadataElementsSource(Logger.GetLogger(typeof (XunitTestElementsSource)), shellLocks);
        }

        public void ExploreSolution(IUnitTestElementsObserver observer)
        {
            // Do nothing. We find all tests via source, or via assembly metadata
        }

        public void ExploreProjects(IDictionary<IProject, FileSystemPath> projects, MetadataLoader loader, IUnitTestElementsObserver observer, CancellationToken cancellationToken)
        {
            var explorer = new XunitTestMetadataExplorer(new UnitTestElementFactory(services, observer.OnUnitTestElementChanged));
            metadataElementsSource.ExploreProjects(projects, loader, observer, explorer.ExploreAssembly, cancellationToken);
            observer.OnCompleted();
        }

        public void ExploreFile(IFile psiFile, IUnitTestElementsObserver observer, Func<bool> interrupted)
        {
            if (!IsProjectFile(psiFile)) return;

            var elementFactory = new UnitTestElementFactory(services, observer.OnUnitTestElementChanged);
            psiFile.ProcessDescendants(new XunitPsiFileExplorer(elementFactory, observer, psiFile, interrupted, searchDomainFactory));
            observer.OnCompleted();
        }

        public IUnitTestProvider Provider
        {
            get { return services.Provider; }
        }

        private static bool IsProjectFile(IFile psiFile)
        {
            // Can return null for external sources
            var projectFile = psiFile.GetSourceFile().ToProjectFile();
            return projectFile != null;
        }
    }
}