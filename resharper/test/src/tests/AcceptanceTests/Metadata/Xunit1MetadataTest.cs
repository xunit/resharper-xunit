using System.Collections.Generic;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Metadata
{
    [Category("xunit1")]
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