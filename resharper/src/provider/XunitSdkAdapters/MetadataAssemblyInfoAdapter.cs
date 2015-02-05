using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.Util;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // TODO: Cache and reuse objects
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
                assemblyQualifiedAttributeTypeName.IndexOf(','));
            return assembly.GetCustomAttributes(fullName)
                .Select(a => (IAttributeInfo)new MetadataAttributeInfoAdapter2(a));
        }

        public ITypeInfo GetType(string typeName)
        {
            var metadataType = assembly.GetTypeFromQualifiedName(typeName, false);

            Assertion.Assert(metadataType is IMetadataClassType, "Expected type to be IMetadataClassType: {0}",
                metadataType.GetType().Name);

            // I'd like to assert that the type is resolved, but it doesn't work for closed generics!
            //Assertion.Assert(metadataType.IsResolved, "Cannot resolve type: {0}", metadataType);
            //if (!metadataType.IsResolved)
            //    return null;

            return new MetadataTypeInfoAdapter2((IMetadataClassType) metadataType);
        }

        public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes)
        {
            // Still don't know why I can't use assembly.GetExportedTypes()
            return from typeInfo in assembly.GetTypes()
                where includePrivateTypes || typeInfo.IsPublic || typeInfo.IsNestedPublic
                select (ITypeInfo)new MetadataTypeInfoAdapter2(typeInfo);
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