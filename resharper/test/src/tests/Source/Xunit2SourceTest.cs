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
                yield return "xunit.abstractions.dll";
                yield return "xunit.core.dll";
                yield return "xunit.execution.dll";
            }
        }
    }
}