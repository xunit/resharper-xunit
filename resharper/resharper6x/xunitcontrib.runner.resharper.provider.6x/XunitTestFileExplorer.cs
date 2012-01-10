using System;
using System.Linq;
using JetBrains.Application;
using JetBrains.Metadata.Utils;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [FileUnitTestExplorer]
    public class XunitTestFileExplorer : IUnitTestFileExplorer
    {
        private readonly XunitTestProvider provider;
        private readonly UnitTestElementFactory unitTestElementFactory;

        public XunitTestFileExplorer(XunitTestProvider provider, UnitTestElementFactory unitTestElementFactory)
        {
            this.provider = provider;
            this.unitTestElementFactory = unitTestElementFactory;
        }

        public void ExploreFile(IFile psiFile, UnitTestElementLocationConsumer consumer, CheckForInterrupt interrupted)
        {
            if (psiFile == null)
                throw new ArgumentNullException("psiFile");

            var project = psiFile.GetProject();
            if (project == null)
                return;

            if (!project.GetAssemblyReferences().Any(IsSilverlightMscorlib))
                return;

            psiFile.ProcessDescendants(new XunitPsiFileExplorer(provider, unitTestElementFactory, consumer, psiFile, interrupted));
        }

        private static bool IsSilverlightMscorlib(IProjectToAssemblyReference reference)
        {
            var assemblyNameInfo = reference.ReferenceTarget.AssemblyName;
            var publicKeyTokenBytes = assemblyNameInfo.GetPublicKeyToken();
            if (publicKeyTokenBytes == null)
                return false;

            var publicKeyToken = AssemblyNameExtensions.GetPublicKeyTokenString(publicKeyTokenBytes);

            // Not sure if this is the best way to do this, but the public key token for mscorlib on
            // the desktop if "b77a5c561934e089". On Silverlight, it's "7cec85d7bea7798e"
            return assemblyNameInfo.Name == "mscorlib" && publicKeyToken == "b77a5c561934e089";
        }

        public IUnitTestProvider Provider
        {
            get { return provider; }
        }
    }
}