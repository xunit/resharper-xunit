using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public interface IGenericArgumentSubstitution
    {
        IEnumerable<ITypeInfo> GenericArguments { get; }
        bool IsGenericMethodDefinition { get; }
        ITypeInfo Apply(IType type);
    }
}