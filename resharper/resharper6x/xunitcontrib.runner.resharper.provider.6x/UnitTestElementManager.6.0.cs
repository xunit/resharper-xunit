using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class UnitTestElementManager
    {
        public IUnitTestElement GetElementById(IProject project, string id)
        {
            return UnitTestManager.GetInstance(project.GetSolution()).GetElementById(project, id);
        }
    }
}