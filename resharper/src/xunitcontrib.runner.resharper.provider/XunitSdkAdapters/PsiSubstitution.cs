using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Util;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class PsiSubstitution : IGenericArgumentSubstitution
    {
        private readonly IAssemblyInfo assembly;
        private readonly IMethod method;
        private readonly ISubstitution substitution;

        public PsiSubstitution(IAssemblyInfo assembly, IMethod method, ISubstitution substitution)
        {
            this.assembly = assembly;
            this.method = method;
            this.substitution = substitution;
        }

        public IEnumerable<ITypeInfo> GenericArguments
        {
            get
            {
                return from typeParameter in method.TypeParameters
                    select (ITypeInfo)new GenericArgumentTypeInfoAdapter(assembly, typeParameter.ShortName);
            }
        }

        public bool IsGenericMethodDefinition
        {
            get { return substitution.IsIdOrEmpty() && method.TypeParameters.Count > 0; }
        }

        public ITypeInfo Apply(IType type)
        {
            return PsiTypeSystem.GetType(substitution[type].GetTypeElement<ITypeElement>());
        }
    }
}