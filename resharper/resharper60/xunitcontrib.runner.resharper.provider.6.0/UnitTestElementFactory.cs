using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class UnitTestElementFactory
    {
        private readonly XunitTestProvider provider;
        private readonly UnitTestElementManager unitTestManager;
        private readonly CacheManager cacheManager;
        private readonly PsiModuleManager psiModuleManager;

        public UnitTestElementFactory(XunitTestProvider provider, UnitTestElementManager unitTestManager, CacheManager cacheManager, PsiModuleManager psiModuleManager)
        {
            this.provider = provider;
            this.unitTestManager = unitTestManager;
            this.cacheManager = cacheManager;
            this.psiModuleManager = psiModuleManager;
        }

        public XunitTestClassElement GetOrCreateTestClass(IProject project, IClrTypeName typeName, string assemblyLocation)
        {
            var id = typeName.FullName;
            var element = unitTestManager.GetElementById(project, id);
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                return element as XunitTestClassElement;
            }

            return new XunitTestClassElement(provider, new ProjectModelElementEnvoy(project), cacheManager, psiModuleManager, typeName.GetPersistent(), assemblyLocation);
        }

        public IUnitTestElement GetOrCreateTestMethod(IProject project, XunitTestClassElement testClassElement, IClrTypeName typeName, string methodName, string skipReason)
        {
            var id = typeName.FullName + "." + methodName;
            var element = unitTestManager.GetElementById(project, id);
            if (element != null)
            {
                element.State = UnitTestElementState.Valid;
                return element as XunitTestMethodElement;
            }

            return new XunitTestMethodElement(provider, testClassElement, new ProjectModelElementEnvoy(project), cacheManager, psiModuleManager, id, typeName, methodName, skipReason);
        }
    }
}