using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Metadata
{
    [Category("xunit1")]
    [Xunit1TestReferences(supportAsync: true)]
    [TestNetFramework4] // For async tests, although it doesn't seem to get picked up
    public class Xunit1MetadataTest : XunitMetadataTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit1"; }
        }
    }
}