using System;
using System.Collections.Generic;

namespace XunitContrib.Runner.ReSharper.Tests.Source
{
    public class Xunit1SourceTest : XunitSourceTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit1"; }
        }

        protected override IEnumerable<string> AssemblyReferences
        {
            get
            {
                yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.dll");
                yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.extensions.dll");
            }
        }
    }
}