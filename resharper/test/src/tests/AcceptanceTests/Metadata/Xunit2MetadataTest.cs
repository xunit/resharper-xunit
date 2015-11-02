using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Metadata
{
    [Category("xunit2")]
    [TestNetFramework45]
    [Xunit2TestReferences]
    public class Xunit2MetadataTest : XunitMetadataTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit2"; }
        }
    }
}