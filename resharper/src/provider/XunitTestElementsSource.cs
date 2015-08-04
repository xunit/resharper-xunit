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
        private readonly XunitTestProvider provider;
        private readonly UnitTestElementFactory unitTestElementFactory;
        private readonly SearchDomainFactory searchDomainFactory;
        private readonly MetadataElementsSource metadataElementsSource;

        public XunitTestElementsSource(XunitTestProvider provider, UnitTestElementFactory unitTestElementFactory,
            SearchDomainFactory searchDomainFactory, IShellLocks shellLocks)
        {
            this.provider = provider;
            this.unitTestElementFactory = unitTestElementFactory;
            this.searchDomainFactory = searchDomainFactory;

            metadataElementsSource = new MetadataElementsSource(Logger.GetLogger(typeof (XunitTestElementsSource)),
                shellLocks);
        }

        public void ExploreSolution(IUnitTestElementsObserver observer)
        {
            // Do nothing. We find all tests via source, or via assembly metadata
        }

#if !RESHARPER92
        public void ExploreProjects(IDictionary<IProject, FileSystemPath> projects, MetadataLoader loader, IUnitTestElementsObserver observer)
#else
        public void ExploreProjects(IDictionary<IProject, FileSystemPath> projects, MetadataLoader loader, IUnitTestElementsObserver observer, CancellationToken cancellationToken)
#endif
        {
            var explorer = new XunitTestMetadataExplorer(provider, unitTestElementFactory);
#if !RESHARPER92
            metadataElementsSource.ExploreProjects(projects, loader, observer, explorer.ExploreAssembly);
#else
            metadataElementsSource.ExploreProjects(projects, loader, observer, explorer.ExploreAssembly, cancellationToken);
#endif
            observer.OnCompleted();
        }

        public void ExploreFile(IFile psiFile, IUnitTestElementsObserver observer, Func<bool> interrupted)
        {
            if (!IsProjectFile(psiFile)) return;

            psiFile.ProcessDescendants(new XunitPsiFileExplorer(provider, unitTestElementFactory, observer,
                psiFile, interrupted, searchDomainFactory));
            observer.OnCompleted();
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }

        private static bool IsProjectFile(IFile psiFile)
        {
            // Can return null for external sources
            var projectFile = psiFile.GetSourceFile().ToProjectFile();
            return projectFile != null;
        }
    }
}