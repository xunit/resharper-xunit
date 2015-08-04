using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Criteria;

namespace JetBrains.ReSharper.UnitTestFramework.Criteria
{
    public interface IUnitTestElementCriterion
    {
    }

    public class NothingCriterion : IUnitTestElementCriterion
    {
        public static readonly NothingCriterion Instance = new NothingCriterion();
    }
}

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public static class UnitTestSessionManagerExtensions
    {
        public static IUnitTestSessionView CreateSession(this IUnitTestSessionManager sessionManager, IUnitTestElementCriterion criterion)
        {
            return sessionManager.CreateSession();
        }
    }
}