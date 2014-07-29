using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class PsiTypeSystem
    {
        public static IAssemblyInfo GetAssembly(IPsiModule module)
        {
            return new PsiAssemblyInfoAdapter(((IProjectPsiModule)module).Project);
        }

        public static ITypeInfo GetType(ITypeElement typeElement)
        {
            if (typeElement is ITypeParameter)
                return new GenericArgumentTypeInfoAdapter(GetAssembly(typeElement.Module), typeElement.ShortName);
            return new PsiTypeInfoAdapter2(typeElement);
        }
    }
}