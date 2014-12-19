using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Source
{
    [Category("xunit2")]
    [TestReferences(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.abstractions.dll",
        EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.core.dll",
        EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.execution.dll")]
    public class Xunit2SourceTest : XunitSourceTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit2"; }
        }
    }
}