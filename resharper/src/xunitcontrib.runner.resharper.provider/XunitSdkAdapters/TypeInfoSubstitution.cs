using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class TypeInfoSubstitution : IGenericArgumentSubstitution
    {
        private readonly IMethod method;
        private readonly ITypeInfo[] typeArguments;

        public TypeInfoSubstitution(IMethod method, ITypeInfo[] typeArguments)
        {
            this.method = method;
            this.typeArguments = typeArguments;
        }

        public IEnumerable<ITypeInfo> GenericArguments
        {
            get { return typeArguments; }
        }

        public bool IsGenericMethodDefinition { get { return false; } }

        public ITypeInfo Apply(IType type)
        {
            var typeElement = type.GetTypeElement<ITypeElement>();
            var name = typeElement.ShortName;
            for (var i = 0; i < method.TypeParameters.Count; i++)
            {
                if (method.TypeParameters[i].ShortName == name)
                    return typeArguments[i];
            }
            return PsiTypeSystem.GetType(typeElement);
        }
    }
}