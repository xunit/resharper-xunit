using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal abstract class XunitTestElement : UnitTestElement
    {
        readonly IProject project;
        readonly string typeName;

        protected XunitTestElement(IUnitTestProvider provider,
                                   UnitTestElement parent,
                                   IProject project,
                                   string typeName)
            : base(provider, parent)
        {
            if (project == null && !Shell.Instance.IsTestShell)
                throw new ArgumentNullException("project");

            if (typeName == null)
                throw new ArgumentNullException("typeName");

            this.project = project;
            this.typeName = typeName;
        }

        protected ITypeElement GetDeclaredType()
        {
            if (project == null)
                return null;

            var solution = project.GetSolution();
            if (solution == null)
                return null;

            var psiManager = PsiManager.GetInstance(solution);

            using (ReadLockCookie.Create())
            {
                var scope = DeclarationsScopeFactory.ModuleScope(PsiModuleManager.GetInstance(solution).GetPrimaryPsiModule(project), true);
                var cache = psiManager.GetDeclarationsCache(scope, true);
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
                                new UnitTestElementLocation(file.ProjectFile, declaration.GetNameDocumentRange().TextRange,
                                                            declaration.GetDocumentRange().TextRange);

            return new UnitTestElementDisposition(locations.ToList(), this);
        }

        public override UnitTestNamespace GetNamespace()
        {
            return new UnitTestNamespace(new CLRTypeName(typeName).NamespaceName);
        }

        public override IProject GetProject()
        {
            return project;
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

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                var element = (XunitTestElement)obj;

                if (Equals(element.project, project))
                    return (element.typeName == typeName);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = base.GetHashCode();
                result = (result * 397) ^ (project != null ? project.GetHashCode() : 0);
                result = (result * 397) ^ (typeName != null ? typeName.GetHashCode() : 0);
                return result;
            }
        }
    }
}