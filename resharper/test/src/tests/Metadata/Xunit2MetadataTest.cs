using System.Collections.Generic;
using JetBrains.ReSharper.TestFramework;

namespace XunitContrib.Runner.ReSharper.Tests.Metadata
{
    [TestNetFramework4]
    [TestReferences("System.Runtime.dll")]
    public class Xunit2MetadataTest : XunitMetadataTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit2"; }
        }

        protected override IEnumerable<string> GetReferencedAssemblies()
        {
            yield return "xunit.abstractions.dll";
            yield return "xunit.core.dll";
            yield return "xunit.execution.dll";
        }
    }
}