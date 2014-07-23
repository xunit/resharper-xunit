using System;
using System.Collections.Generic;
using JetBrains.ProjectModel;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class PsiAssemblyInfoAdapter : IAssemblyInfo
    {
        // TODO: Should this be an envoy?
        private readonly IProject project;

        public PsiAssemblyInfoAdapter(IProject project)
        {
            this.project = project;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            // Requires pulling all assembly attributes from source. Need to
            // scan the WHOLE project. I feel an ICache coming on
            throw new NotImplementedException();
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