using System;
using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.Application;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper
{
    public abstract class XunitTestElement : UnitTestElement
    {
        readonly ProjectModelElementEnvoy projectEnvoy;
        readonly string typeName;

        protected XunitTestElement(IUnitTestProvider provider,
                                   UnitTestElement parent,
                                   IProjectModelElement project,
                                   string typeName)
            : base(provider, parent)
        {
            if (project == null && !Shell.Instance.IsTestShell)
                throw new ArgumentNullException("project");

            if (typeName == null)
                throw new ArgumentNullException("typeName");

            if (project != null)
                projectEnvoy = new ProjectModelElementEnvoy(project);

            this.typeName = typeName;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                XunitTestElement element = (XunitTestElement)obj;

                if (Equals(element.projectEnvoy, projectEnvoy))
                    return (element.typeName == typeName);
            }

            return false;
        }

        protected ITypeElement GetDeclaredType()
        {
            IProject project = GetProject();
            if (project == null)
                return null;

            PsiManager manager = PsiManager.GetInstance(project.GetSolution());

            IList<IPsiModule> modules = PsiModuleManager.GetInstance(projectEnvoy.Solution).GetPsiModules(project);
            IPsiModule projectModule = modules.Count > 0 ? modules[0] : null;

            using (ReadLockCookie.Create())
            {
                IDeclarationsScope scope = DeclarationsScopeFactory.ModuleScope(projectModule, false);
                IDeclarationsCache cache = manager.GetDeclarationsCache(scope, true);
                return cache.GetTypeElementByCLRName(typeName);
            }
        }

        public override UnitTestElementDisposition GetDisposition()
        {
            IDeclaredElement element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            List<UnitTestElementLocation> locations = new List<UnitTestElementLocation>();

            foreach (IDeclaration declaration in element.GetDeclarations())
            {
                IFile file = declaration.GetContainingFile();

                if (file != null)
                    locations.Add(new UnitTestElementLocation(file.ProjectFile,
                                                              declaration.GetNameRange(),
                                                              declaration.GetDocumentRange().TextRange));
            }

            return new UnitTestElementDisposition(locations, this);
        }

        public override UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(new CLRTypeName(typeName).NamespaceName);
        }

        public override IProject GetProject()
        {
            return projectEnvoy.GetValidProjectElement() as IProject;
        }

        public override IList<IProjectFile> GetProjectFiles()
        {
            ITypeElement type = GetDeclaredType();
            if (type == null)
                return EmptyArray<IProjectFile>.Instance;

            return type.GetProjectFiles();
        }

        public override string GetTypeClrName()
        {
            return typeName;
        }
    }
}