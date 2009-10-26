using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Application;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
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
                var element = (XunitTestElement)obj;

                if (Equals(element.projectEnvoy, projectEnvoy))
                    return (element.typeName == typeName);
            }

            return false;
        }

        protected ITypeElement GetDeclaredType()
        {
            var project = GetProject();
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
            var element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            var locations = (from declaration in element.GetDeclarations()
                             let file = declaration.GetContainingFile()
                             where file != null
                             select new UnitTestElementLocation(file.ProjectFile, declaration.GetNameDocumentRange().TextRange, declaration.GetDocumentRange().TextRange)).ToList();

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
            var type = GetDeclaredType();
            return type == null ? EmptyArray<IProjectFile>.Instance : type.GetProjectFiles();
        }

        public override string GetTypeClrName()
        {
            return typeName;
        }
    }
}