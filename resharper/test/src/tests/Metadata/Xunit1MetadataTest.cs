using System.Collections.Generic;
using XunitContrib.Runner.ReSharper.Tests.Properties;

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
            yield return EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.dll";
            yield return EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.extensions.dll";
        }
    }
}