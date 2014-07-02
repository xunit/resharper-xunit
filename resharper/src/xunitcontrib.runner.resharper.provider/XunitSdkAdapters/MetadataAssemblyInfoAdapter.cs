using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class MetadataAssemblyInfoAdapter : IAssemblyInfo
    {
        private readonly IMetadataAssembly assembly;

        public MetadataAssemblyInfoAdapter(IMetadataAssembly assembly)
        {
            this.assembly = assembly;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var fullName = assemblyQualifiedAttributeTypeName.Substring(0,
                assemblyQualifiedAttributeTypeName.IndexOf(","));
            return assembly.GetCustomAttributes(fullName)
                .Select(a => (IAttributeInfo)new MetadataAttributeInfoAdapter2(a));
        }

        public ITypeInfo GetType(string typeName)
        {
            var typeInfo = assembly.GetTypeInfoFromQualifiedName(typeName, false);
            return new MetadataTypeInfoAdapter2(typeInfo);
        }

        public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
        {
            // Still don't know why I can't use assembly.GetExportedTypes()
            return from t in assembly.GetTypes()
                where includePrivateTypes || t.IsPublic || t.IsNestedPublic
                select (ITypeInfo)new MetadataTypeInfoAdapter2(t);
        }

        public string AssemblyPath
        {
            get { return assembly.Location.FullPath; }
        }

        public string Name
        {
            get { return assembly.AssemblyName.FullName; }
        }
    }
}