using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class UnitTestElementManager
    {
        private readonly IUnitTestElementManager unitTestElementManager;

        public UnitTestElementManager(IUnitTestElementManager unitTestElementManager)
        {
            this.unitTestElementManager = unitTestElementManager;
        }

        public IUnitTestElement GetElementById(IProject project, string id)
        {
            return unitTestElementManager.GetElementById(project, id);
        }
    }
}