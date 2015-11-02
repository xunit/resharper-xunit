using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Source
{
    [Category("xunit1")]
    [TestNetFramework35]
    [Xunit1TestReferences]
    public class Xunit1SourceTest : XunitSourceTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit1"; }
        }
    }
}