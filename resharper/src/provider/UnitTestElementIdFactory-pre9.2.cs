using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public interface IUnitTestElementIdFactory
    {
        UnitTestElementId Create(IUnitTestProvider provider, PersistentProjectId persistentProjectId, string id);
    }

    [SolutionComponent]
    public class UnitTestElementIdFactory : IUnitTestElementIdFactory
    {
        public UnitTestElementId Create(IUnitTestProvider provider, PersistentProjectId persistentProjectId, string id)
        {
            return new UnitTestElementId(provider, persistentProjectId, id);
        }
    }
}