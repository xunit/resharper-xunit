using System.Collections.Generic;
using JetBrains.Metadata.Reader.API;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class MetadataMethodInfoAdapter2 : IMethodInfo
    {
        private readonly IMetadataMethod method;

        public MetadataMethodInfoAdapter2(IMetadataMethod method)
        {
            this.method = method;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IParameterInfo> GetParameters()
        {
            throw new System.NotImplementedException();
        }

        public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
        {
            throw new System.NotImplementedException();
        }

        public bool IsAbstract { get; private set; }
        public bool IsGenericMethodDefinition { get; private set; }
        public bool IsPublic { get; private set; }
        public bool IsStatic { get; private set; }
        public string Name { get { return method.Name; } }
        public ITypeInfo ReturnType { get; private set; }
        public ITypeInfo Type { get; private set; }
    }
}