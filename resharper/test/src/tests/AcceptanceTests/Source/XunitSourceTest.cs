using System.Collections.Generic;
using System.Linq;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Source
{
    [Category("Source discovery")]
    public abstract partial class XunitSourceTest : XunitSourceTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Exploration\" + RelativeTestDataPathSuffix; }
        }

        protected abstract string RelativeTestDataPathSuffix { get; }

        [TestCaseSource("GetAllCSharpFilesInDirectory")]
        public void TestFile(string filename)
        {
            DoTestSolution(GetTestDataFilePath(filename));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IEnumerable<string> GetAllCSharpFilesInDirectory()
        {
            InferProductHomeDir();
            return TestDataPath2.GetChildFiles("*.cs").Select(path => path.Name);
        }

        partial void InferProductHomeDir();
    }
}