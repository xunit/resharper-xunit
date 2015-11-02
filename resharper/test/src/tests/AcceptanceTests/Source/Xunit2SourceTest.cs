using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Source
{
    [Category("xunit2")]
    [TestNetFramework45]
    [Xunit2TestReferences]
    public class Xunit2SourceTest : XunitSourceTest
    {

        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit2"; }
        }
    }
}