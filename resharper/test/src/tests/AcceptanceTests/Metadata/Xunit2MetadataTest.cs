using System.Collections.Generic;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Metadata
{
    [Category("xunit2")]
    [TestNetFramework4]
    public class Xunit2MetadataTest : XunitMetadataTest
    {
        private readonly IXunitEnvironment environment = new Xunit2Environment();

        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit2"; }
        }

        protected override IEnumerable<string> GetReferencedAssemblies()
        {
            return environment.GetReferences(environment.GetPlatformID(), TestDataPath2);
        }
    }
}