using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.Application;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal abstract class XunitTestElement : UnitTestElement
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

            var manager = PsiManager.GetInstance(project.GetSolution());

            var modules = PsiModuleManager.GetInstance(projectEnvoy.Solution).GetPsiModules(project);
            var projectModule = modules.Count > 0 ? modules[0] : null;

            using (ReadLockCookie.Create())
            {
                var scope = DeclarationsScopeFactory.ModuleScope(projectModule, false);
                var cache = manager.GetDeclarationsCache(scope, true);
                return cache.GetTypeElementByCLRName(typeName);
            }
        }

        public override UnitTestElementDisposition GetDisposition()
        {
            var element = GetDeclaredElement();
            if (element == null || !element.IsValid())
                return UnitTestElementDisposition.InvalidDisposition;

            var locations = from declaration in element.GetDeclarations()
                            let file = declaration.GetContainingFile()
                            where file != null
                            select
                                new UnitTestElementLocation(file.ProjectFile, declaration.GetNameRange(),
                                                            declaration.GetDocumentRange().TextRange);

            return new UnitTestElementDisposition(locations.ToList(), this);
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