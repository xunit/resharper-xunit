using System.Collections.Generic;
using JetBrains.Util;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class GenericArgumentTypeInfoAdapter : ITypeInfo
    {
        public GenericArgumentTypeInfoAdapter(IAssemblyInfo assembly, string name)
        {
            Assembly = assembly;
            Name = name;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return EmptyArray<IAttributeInfo>.Instance;
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            return EmptyArray<ITypeInfo>.Instance;
        }

        public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
        {
            return null;
        }

        public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
        {
            return EmptyArray<IMethodInfo>.Instance;
        }

        public IAssemblyInfo Assembly { get; private set; }
        public ITypeInfo BaseType { get { return null; } }
        public IEnumerable<ITypeInfo> Interfaces { get { return EmptyArray<ITypeInfo>.Instance; } }

        // Based on Reflection answers
        public bool IsAbstract { get { return true; } }
        public bool IsGenericParameter { get { return true; } }
        public bool IsGenericType { get { return false; } }
        public bool IsSealed { get { return true; } }
        public bool IsValueType { get { return false; } }

        public string Name { get; private set; }
    }
}