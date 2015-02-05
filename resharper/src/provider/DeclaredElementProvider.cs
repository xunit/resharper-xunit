using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class DeclaredElementProvider
    {
        private readonly IPsiServices psiServices;

        public DeclaredElementProvider(IPsiServices psiServices)
        {
            this.psiServices = psiServices;
        }

        public IDeclaredElement GetDeclaredElement(IProject project, IClrTypeName typeName)
        {
            var modules = psiServices.Modules;
            var psiModule = modules.GetPrimaryPsiModule(project);
            if (psiModule == null)
                return null;

            var symbolScope = psiServices.Symbols.GetSymbolScope(psiModule, project.GetResolveContext(), true, true);
            return symbolScope.GetTypeElementByCLRName(typeName);
        }
    }
}