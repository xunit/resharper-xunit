using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class DeclaredElementProvider
    {
        private readonly PsiModuleManager psiModuleManager;
        private readonly CacheManager cacheManager;

        public DeclaredElementProvider(PsiModuleManager psiModuleManager, CacheManager cacheManager)
        {
            this.psiModuleManager = psiModuleManager;
            this.cacheManager = cacheManager;
        }

        public IDeclaredElement GetDeclaredElement(IProject project, IClrTypeName typeName)
        {
            var psiModule = psiModuleManager.GetPrimaryPsiModule(project);
            return psiModule == null ? null : cacheManager.GetDeclarationsCache(psiModule, false, true).GetTypeElementByCLRName(typeName);
        }
    }
}