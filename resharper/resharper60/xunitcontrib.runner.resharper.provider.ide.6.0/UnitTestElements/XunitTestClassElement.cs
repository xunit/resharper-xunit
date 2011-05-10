using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TaskRunnerFramework.UnitTesting;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.Psi.Tree;
using XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.UnitTestRunnerElements;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitTestClassElement : XunitTestRunnerClassElement, IUnitTestElement, ISerializableUnitTestElement
    {
        private readonly IProjectModelElementPointer projectPointer;

        public XunitTestClassElement(IUnitTestRunnerProvider provider, IProject project, string typeName, string shortName, string assemblyLocation)
            : base(provider, typeName, shortName, assemblyLocation)
        {
            projectPointer = project.CreatePointer();
            TypeName = typeName;
        }

        public void WriteToXml(XmlElement parent)
        {
            parent.SetAttribute("id", Id);
            parent.SetAttribute("projectId", GetProject().GetPersistentID());
            parent.SetAttribute("typeName", TypeName);
        }

        internal static IUnitTestElement ReadFromXml(XmlElement parent, IUnitTestElement parentElement, IUnitTestProvider provider)
        {
            var id = parent.GetAttribute("id");
            var projectId = parent.GetAttribute("projectId");
            var typeName = parent.GetAttribute("typeName");

            var shortName = new ClrTypeName(typeName).ShortName;

            var project = (IProject) ProjectUtil.FindProjectElementByPersistentID(provider.Solution, projectId);
            if (project == null)
                return null;
            var assemblyLocation = UnitTestManager.GetOutputAssemblyPath(project).FullPath;

            var manager = UnitTestManager.GetInstance(provider.Solution);
            var element = manager.GetOrCreateElementById(project, id, () => new XunitTestClassElement(provider, project, typeName, shortName, assemblyLocation));
            if (element == null)
                return new XunitTestClassElement(provider, project, typeName, shortName, assemblyLocation);

            element.State = UnitTestElementState.Valid;
            // TODO: Remove invalid children?

            return element as IUnitTestElement;
        }

        public bool Equals(IUnitTestElement other)
        {
            return Equals(other as XunitTestClassElement);
        }

        public bool Equals(XunitTestClassElement other)
        {
            return other != null && base.Equals(other) && other.TypeName == TypeName;
        }

        public IProject GetProject()
        {
            return (projectPointer.GetValidProjectElement(((XunitTestProvider)Provider).Solution) as IProject);
        }

        public IProjectModelElementPointer GetProjectPointer()
        {
            return projectPointer;
        }

        public string GetPresentation()
        {
            return ShortName;
        }

        public UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(new ClrTypeName(TypeName).GetNamespaceName());
        }

        public UnitTestElementDisposition GetDisposition()
        {
            var element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            var locations = from declaration in element.GetDeclarations()
                            let file = declaration.GetContainingFile()
                            where file != null
                            select new UnitTestElementLocation(file.GetSourceFile().ToProjectFile(),
                                                               declaration.GetNameDocumentRange().TextRange,
                                                               declaration.GetDocumentRange().TextRange);
            return new UnitTestElementDisposition(locations, this);
        }

        public IDeclaredElement GetDeclaredElement()
        {
            var p = GetProject();
            if (p == null)
                return null;

            var provider = (XunitTestProvider) Provider;

            var psiModule = provider.PsiModuleManager.GetPrimaryPsiModule(p);
            if (psiModule == null)
                return null;

            return provider.CacheManager.GetDeclarationsCache(psiModule, false, true).GetTypeElementByCLRName(TypeName);
        }

        public string Kind
        {
            get { return "xUnit.net Test Class"; }
        }

        public IEnumerable<UnitTestElementCategory> Categories
        {
            get { return Enumerable.Empty<UnitTestElementCategory>(); }
        }

        // xunit doesn't support class level skip
        public string ExplicitReason
        {
            get { return string.Empty; }
        }
    }
}