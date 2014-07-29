using JetBrains.ReSharper.Psi;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class PsiParameterInfoAdapter : IParameterInfo
    {
        private readonly IParameter parameter;
        private readonly IGenericArgumentSubstitution substitution;

        public PsiParameterInfoAdapter(IParameter parameter, IGenericArgumentSubstitution substitution)
        {
            this.parameter = parameter;
            this.substitution = substitution;
        }

        public string Name { get { return parameter.ShortName; } }

        public ITypeInfo ParameterType
        {
            get { return substitution.Apply(parameter.Type); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}