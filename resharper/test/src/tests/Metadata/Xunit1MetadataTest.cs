using System.Collections.Generic;

namespace XunitContrib.Runner.ReSharper.Tests.Metadata
{
    public class Xunit1MetadataTest : XunitMetadataTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit1"; }
        }

        protected override IEnumerable<string> GetReferencedAssemblies()
        {
            yield return "xunit.dll";
            yield return "xunit.extensions.dll";
        }
    }
}