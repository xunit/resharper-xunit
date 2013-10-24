using System;
using JetBrains.Application;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.UnitTestFramework;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public partial class XunitTestFileExplorer
    {
        // ReSharper 8.1 replaced CheckForInterrupt with Func<bool>
        public void ExploreFile(IFile psiFile, UnitTestElementLocationConsumer consumer, CheckForInterrupt interrupted)
        {
            ExploreFile(psiFile, consumer, new Func<bool>(() => interrupted()));
        }
    }
}