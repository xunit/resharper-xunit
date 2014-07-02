using System.Collections.Generic;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Metadata
{
    [Category("xunit2")]
    public class Xunit2MetadataTest : XunitMetadataTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit2"; }
        }

        protected override IEnumerable<string> GetReferencedAssemblies()
        {
            yield return "System.Runtime.dll";
            yield return EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.abstractions.dll";
            yield return EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.core.dll";
            yield return EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.execution.dll";
            yield return EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.assert.dll";
        }
    }
}