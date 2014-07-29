using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class PsiMethodInfoAdapter2 : IMethodInfo
    {
        private readonly IMethod method;
        private readonly IGenericArgumentSubstitution substitution;

        public PsiMethodInfoAdapter2(ITypeInfo typeInfo, IMethod method, IGenericArgumentSubstitution substitution)
        {
            Type = typeInfo;
            this.method = method;
            this.substitution = substitution;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var fullName = assemblyQualifiedAttributeTypeName;
            return from a in method.GetAttributeInstances(new ClrTypeName(fullName), true)
                select (IAttributeInfo) new PsiAttributeInfoAdapter2(a);
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            return substitution.GenericArguments;
        }

        public IEnumerable<IParameterInfo> GetParameters()
        {
            return from p in method.Parameters
                select (IParameterInfo) new PsiParameterInfoAdapter(p, substitution);
        }

        public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
        {
            if (!IsGenericMethodDefinition)
                throw new InvalidOperationException("Current type is not open generic!");
            if (typeArguments == null || typeArguments.Length != method.TypeParameters.Count)
                throw new ArgumentException("TypeArguments cannot be null and must be as long as GenericArguments", "typeArguments");

            var newSubstitutions = new TypeInfoSubstitution(method, typeArguments);
            return new PsiMethodInfoAdapter2(Type, method, newSubstitutions);
        }

        public bool IsAbstract { get { return method.IsAbstract; } }
        public bool IsGenericMethodDefinition { get { return substitution.IsGenericMethodDefinition; } }
        public bool IsPublic { get { return method.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC; } }
        public bool IsStatic { get { return method.IsStatic; } }
        public string Name { get { return method.ShortName; } }

        public ITypeInfo ReturnType
        {
            get { return substitution.Apply(method.ReturnType); }
        }

        public ITypeInfo Type { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}