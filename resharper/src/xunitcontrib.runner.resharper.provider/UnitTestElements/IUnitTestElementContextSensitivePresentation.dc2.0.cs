using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public interface IUnitTestElementContextSensitivePresentation
    {
        string GetPresentation(IUnitTestElement parentElement);
    }
}