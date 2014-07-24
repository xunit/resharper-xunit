using System;
using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class PsiTypeInfoAdapter2 : ITypeInfo
    {
        private readonly ITypeElement psiType;
        private readonly Lazy<string> name; 

        public PsiTypeInfoAdapter2(ITypeElement psiType)
        {
            this.psiType = psiType;
            name = new Lazy<string>(() => psiType.GetClrName().FullName);
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            throw new NotImplementedException();
        }

        public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
        {
            throw new NotImplementedException();
        }

        public IAssemblyInfo Assembly { get; private set; }
        public ITypeInfo BaseType { get; private set; }
        public IEnumerable<ITypeInfo> Interfaces { get; private set; }
        public bool IsAbstract { get; private set; }
        public bool IsGenericParameter { get; private set; }
        public bool IsGenericType { get; private set; }
        public bool IsSealed { get; private set; }
        public bool IsValueType { get; private set; }
        public string Name { get { return name.Value; } }
    }
}