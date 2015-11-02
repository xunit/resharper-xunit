using System.Collections.Generic;
using System.Linq;
using JetBrains.TestFramework.Utils;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Source
{
    [Category("Source discovery")]
    public abstract class XunitSourceTest : XunitSourceTestBase
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

        private void InferProductHomeDir()
        {
            // TestCases is called before the environment fixture is run, which would either 
            // ensure Product.Root exists, or infer %JetProductHomeDir%. By using SoutionItemsBasePath 
            // in TestCases, we fallback to the non-extension friendly implementation that expects 
            // the source to be laid out as if we're building the product, not an extension, and it 
            // requires Product.Root. We'll infer it instead. 
            TestUtil.SetHomeDir(GetType().Assembly);
        }
    }
}