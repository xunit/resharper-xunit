using System;
using System.Collections.Generic;

namespace XunitContrib.Runner.ReSharper.Tests.Source
{
    public class Xunit2SourceTest : XunitSourceTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit2"; }
        }

        protected override IEnumerable<string> AssemblyReferences
        {
            get
            {
                yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.abstractions.dll");
                yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.core.dll");
                yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.execution.dll");
            }
        }
    }
}