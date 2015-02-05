using System;
using System.Linq;
using JetBrains.Metadata.Utils;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [FileUnitTestExplorer]
    public partial class XunitTestFileExplorer : IUnitTestFileExplorer
    {
        private const string SilverlightMscorlibPublicKeyToken = "7cec85d7bea7798e";
        private const string AgUnitProviderId = "Silverlight";

        private readonly XunitTestProvider provider;
        private readonly UnitTestElementFactory unitTestElementFactory;
        private readonly SearchDomainFactory searchDomainFactory;
        private readonly UnitTestProviders providers;

        public XunitTestFileExplorer(XunitTestProvider provider, UnitTestProviders providers,
                                     UnitTestElementFactory unitTestElementFactory,
                                     SearchDomainFactory searchDomainFactory)
        {
            this.provider = provider;
            this.unitTestElementFactory = unitTestElementFactory;
            this.searchDomainFactory = searchDomainFactory;
            this.providers = providers;
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }

        public void ExploreFile(IFile psiFile, UnitTestElementLocationConsumer consumer, Func<bool> interrupted)
        {
            if (psiFile == null)
                throw new ArgumentNullException("psiFile");

            var project = psiFile.GetProject();
            if (project == null)
                return;

            if (IsSilverlightProject(project) && !IsAgUnitAvailable)
                return;

            var observer = new UnitTestElementsObserver(consumer);
            psiFile.ProcessDescendants(new XunitPsiFileExplorer(provider, unitTestElementFactory, observer, psiFile, interrupted, searchDomainFactory));
        }

        private static bool IsSilverlightProject(IProject project)
        {
            return project.GetAssemblyReferences().Any(IsSilverlightMscorlib);
        }

        private static bool IsSilverlightMscorlib(IProjectToAssemblyReference reference)
        {
            var assemblyNameInfo = reference.ReferenceTarget.AssemblyName;
            var publicKeyTokenBytes = assemblyNameInfo.GetPublicKeyToken();
            if (publicKeyTokenBytes == null)
                return false;

            // Not sure if this is the best way to do this, but the public key token for mscorlib on
            // the desktop if "b77a5c561934e089". On Silverlight, it's "7cec85d7bea7798e"
            var publicKeyToken = AssemblyNameExtensions.GetPublicKeyTokenString(publicKeyTokenBytes);
            return assemblyNameInfo.Name == "mscorlib" && publicKeyToken == SilverlightMscorlibPublicKeyToken;
        }

        protected bool IsAgUnitAvailable
        {
            get { return providers.GetProviders().Any(p => p.ID == AgUnitProviderId); }
        }
    }
}