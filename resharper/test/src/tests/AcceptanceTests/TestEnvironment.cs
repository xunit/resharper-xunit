using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

// Hmm. 9.2 seems to require STA - ShellLocks asserts if we're not. I don't know why, yet.
[assembly: RequiresSTA]

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
