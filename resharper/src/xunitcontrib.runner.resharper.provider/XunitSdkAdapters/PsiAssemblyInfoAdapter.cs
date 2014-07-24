using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve.Filters;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class PsiAssemblyInfoAdapter : IAssemblyInfo
    {
        // TODO: Should this be an envoy?
        private readonly IProject project;
        private readonly IPsiServices psiServices;

        public PsiAssemblyInfoAdapter(IProject project)
        {
            this.project = project;
            psiServices = project.GetSolution().GetPsiServices();
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var fullName = assemblyQualifiedAttributeTypeName.Substring(0,
                assemblyQualifiedAttributeTypeName.IndexOf(','));

            foreach (var psiModule in project.GetPsiModules())
            {
                var attributesSet = psiServices.Symbols.GetModuleAttributes(psiModule, project.GetResolveContext());
                if (attributesSet == null)
                    continue;

                IList<IAttributeInstance> attributes;
                using (ReadLockCookie.Create())
                    attributes = attributesSet.GetAttributeInstances(new ClrTypeName(fullName), true);

                foreach (var attribute in attributes)
                    yield return new PsiAttributeInfoAdapter2(attribute);
            }
        }

        public ITypeInfo GetType(string typeName)
        {
            var symbolScope = GetModuleSymbolScope();

            // TODO: Does this handle partial types?
            // TODO: Document in devguide
            var typeElement = symbolScope.GetTypeElementByCLRName(typeName);
            if (typeElement != null)
                return new PsiTypeInfoAdapter2(typeElement);
            return null;
        }

        public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
        {
            // TODO: Document in devguide. Also, is this the best way?
            var symbolCache = psiServices.Symbols;
            return from sourceFile in GetPrimaryModule().SourceFiles
                from typeElement in symbolCache.GetTypesAndNamespacesInFile(sourceFile).OfType<ITypeElement>()
                where includePrivateTypes || IsPublic(typeElement)
                select (ITypeInfo) new PsiTypeInfoAdapter2(typeElement);
        }

        public string AssemblyPath { get { return project.GetOutputFilePath().FullPath; } }
        public string Name { get { return project.GetOutputAssemblyName(); } }

        private ISymbolScope GetModuleSymbolScope()
        {
            var module = GetPrimaryModule();
            // TODO: How to tell if it's case sensitive or not?
            return psiServices.Symbols.GetSymbolScope(module, project.GetResolveContext(), false, true);
        }

        private IPsiModule GetPrimaryModule()
        {
            return psiServices.Modules.GetPrimaryPsiModule(project);
        }

        private static bool IsPublic(ITypeElement typeElement)
        {
            var typeMember = typeElement as ITypeMember;
            if (typeMember != null)
                return typeMember.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC;

            var accessRightsOwner = typeElement as IAccessRightsOwner;
            if (accessRightsOwner != null)
                return accessRightsOwner.GetAccessRights() == AccessRights.PUBLIC;

            // Shouldn't happen?
            return false;
        }
    }
}