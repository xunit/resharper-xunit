using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    [ZoneDefinition]
    public interface IXunitTestZone : ITestsZone, IRequire<PsiFeatureTestZone>
    {
    }

    [SetUpFixture]
    public class TestEnvironmentAssembly : ExtensionTestEnvironmentAssembly<IXunitTestZone>
    {
    }
}
