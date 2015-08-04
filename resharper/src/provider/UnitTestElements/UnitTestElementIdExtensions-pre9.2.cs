using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class UnitTestElementIdExtensions
    {
        public static string GetPersistentProjectId(this UnitTestElementId id)
        {
            return id.PersistentProjectId.Id;
        }
    }
}