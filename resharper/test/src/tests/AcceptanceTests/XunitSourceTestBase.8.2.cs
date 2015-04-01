using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestFramework;
using XunitContrib.Runner.ReSharper.UnitTestProvider;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract partial class XunitSourceTestBase
    {
        protected override IUnitTestFileExplorer FileExplorer
        {
            get { return Solution.GetComponent<XunitTestFileExplorer>(); }
        }

        protected override IEnumerable<string> AssemblyReferences
        {
            get { return GetReferencedAssemblies(); }
        }
    }
}