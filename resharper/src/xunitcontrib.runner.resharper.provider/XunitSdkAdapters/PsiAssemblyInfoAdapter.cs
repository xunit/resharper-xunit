using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
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
            // How to get a sepcific type declaration? Useful to document
            // for the devguide
            throw new NotImplementedException();
        }

        public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
        {
            // How to get all types in a project? Document for the devguide
            throw new NotImplementedException();
        }

        public string AssemblyPath { get { return project.GetOutputFilePath().FullPath; } }
        public string Name { get { return project.GetOutputAssemblyName(); } }
    }
}