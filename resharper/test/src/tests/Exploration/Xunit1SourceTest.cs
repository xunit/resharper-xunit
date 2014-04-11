using System.Collections.Generic;

namespace XunitContrib.Runner.ReSharper.Tests.Exploration
{
    public class Xunit1SourceTest : XunitSourceTest
    {
        protected override IEnumerable<string> AssemblyReferences
        {
            get
            {
                yield return "xunit.dll";
                yield return "xunit.extensions.dll";
            }
        }

        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit1"; }
        }
    }
}