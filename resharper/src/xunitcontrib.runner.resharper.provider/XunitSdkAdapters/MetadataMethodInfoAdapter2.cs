using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class MetadataMethodInfoAdapter2 : IMethodInfo
    {
        private readonly MetadataTypeInfoAdapter2 typeInfo;
        private readonly IMetadataMethod method;
        private readonly ITypeInfo[] substitutions;

        public MetadataMethodInfoAdapter2(MetadataTypeInfoAdapter2 typeInfo, IMetadataMethod method)
            : this(typeInfo, method, null)
        {
        }

        private MetadataMethodInfoAdapter2(MetadataTypeInfoAdapter2 typeInfo, IMetadataMethod method,
            ITypeInfo[] substitutions)
        {
            this.typeInfo = typeInfo;
            this.method = method;
            this.substitutions = substitutions;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var fullName = assemblyQualifiedAttributeTypeName.Substring(0,
                assemblyQualifiedAttributeTypeName.IndexOf(','));
            return method.GetCustomAttributes(fullName)
                .Select(a => (IAttributeInfo)new MetadataAttributeInfoAdapter2(a));
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            if (substitutions != null)
                return substitutions;

            // IMetadataGenericArgument doesn't really give us enough to build an ITypeInfo with,
            // so create a new type with some default values
            return from a in method.GenericArguments
                select (ITypeInfo) new GenericArgumentTypeInfoAdapter(typeInfo.Assembly, a.Name);
        }

        public IEnumerable<IParameterInfo> GetParameters()
        {
            return from p in method.Parameters
                select (IParameterInfo) new MetadataParameterInfoAdapter(typeInfo, this, p);
        }

        public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
        {
            if (!IsGenericMethodDefinition)
                throw new InvalidOperationException("Current type is not open generic!");
            if (typeArguments == null || typeArguments.Length != method.GenericArguments.Length)
                throw new ArgumentException("TypeArguments cannot be null and must be as long as GenericArguments", "typeArguments");

            return new MetadataMethodInfoAdapter2(typeInfo, method, typeArguments);
        }

        public bool IsAbstract { get { return method.IsAbstract; } }

        public bool IsGenericMethodDefinition
        {
            get { return substitutions == null && method.GenericArguments.Length > 0; }
        }

        public bool IsPublic { get { return method.IsPublic; } }
        public bool IsStatic { get { return method.IsStatic; } }
        public string Name { get { return method.Name; } }

        public ITypeInfo ReturnType
        {
            get { return MetadataTypeSystem.GetType(method.ReturnValue.Type, typeInfo, this); }
        }

        public ITypeInfo Type { get { return typeInfo; } }
    }
}