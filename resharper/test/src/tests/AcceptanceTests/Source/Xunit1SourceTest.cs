using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Source
{
    [Category("xunit1")]
    [TestReferences(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.dll",
        EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.extensions.dll")]
    public class Xunit1SourceTest : XunitSourceTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit1"; }
        }
    }
}