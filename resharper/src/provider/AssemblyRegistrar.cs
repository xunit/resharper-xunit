using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.RemoteRunner;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    [SolutionComponent]
    public class AssemblyRegistrar
    {
        public AssemblyRegistrar(UnitTestingAssemblyLoader loader)
        {
            loader.RegisterAssembly(typeof(XunitTaskRunner).Assembly);
        }
    }
}