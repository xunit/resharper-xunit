using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.Util;
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
                select (ITypeInfo) new MetadataGenericArgumentTypeInfoAdapter(typeInfo.Assembly, a);
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

        private class MetadataGenericArgumentTypeInfoAdapter : ITypeInfo
        {
            private readonly IMetadataGenericArgument argument;

            public MetadataGenericArgumentTypeInfoAdapter(IAssemblyInfo assembly, IMetadataGenericArgument argument)
            {
                Assembly = assembly;
                this.argument = argument;
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
            {
                return EmptyList<IAttributeInfo>.InstanceList;
            }

            public IEnumerable<ITypeInfo> GetGenericArguments()
            {
                return EmptyList<ITypeInfo>.InstanceList;
            }

            public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
            {
                return null;
            }

            public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
            {
                return EmptyList<IMethodInfo>.InstanceList;
            }

            public IAssemblyInfo Assembly { get; private set; }
            public ITypeInfo BaseType { get { return null; } }
            public IEnumerable<ITypeInfo> Interfaces { get { return EmptyList<ITypeInfo>.InstanceList; } }
            public bool IsAbstract { get { return true; } }
            public bool IsGenericParameter { get { return true; } }
            public bool IsGenericType { get { return false; } }
            public bool IsSealed { get { return true; } }
            public bool IsValueType { get { return false; } }
            public string Name { get { return argument.Name; } }
        }
    }
}