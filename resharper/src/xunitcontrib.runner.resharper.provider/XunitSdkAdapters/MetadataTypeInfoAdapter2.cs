using System.Collections.Generic;
using JetBrains.Metadata.Reader.API;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class MetadataTypeInfoAdapter2 : ITypeInfo
    {
        private readonly IMetadataTypeInfo typeInfo;

        public MetadataTypeInfoAdapter2(IMetadataTypeInfo typeInfo)
        {
            this.typeInfo = typeInfo;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            throw new System.NotImplementedException();
        }

        public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
        {
            throw new System.NotImplementedException();
        }

        public IAssemblyInfo Assembly { get; private set; }
        public ITypeInfo BaseType { get; private set; }
        public IEnumerable<ITypeInfo> Interfaces { get; private set; }
        public bool IsAbstract { get; private set; }
        public bool IsGenericParameter { get; private set; }
        public bool IsGenericType { get; private set; }
        public bool IsSealed { get; private set; }
        public bool IsValueType { get; private set; }
        public string Name { get { return typeInfo.FullyQualifiedName; } }
    }
}