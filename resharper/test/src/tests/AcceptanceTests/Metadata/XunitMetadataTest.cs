using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Components;
using JetBrains.Application.platforms;
using JetBrains.TestFramework.Utils;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Metadata
{
    [Category("Metadata discovery")]
    public abstract class XunitMetadataTest : XunitMetadataTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Exploration\" + RelativeTestDataPathSuffix; }
        }

        protected abstract string RelativeTestDataPathSuffix { get; }

        [TestCaseSource("GetAllSourceFilesInTestDirectory")]
        public void TestFile(string filename)
        {
            DoTestSolution(GetDll(filename));
        }

        private string GetDll(string filename)
        {
            var source = GetTestDataFilePath2(filename);
            var dll = source.ChangeExtension("dll");

            if (!dll.ExistsFile || source.FileModificationTimeUtc > dll.FileModificationTimeUtc)
            {
                var references = GetReferencedAssemblies().ToArray();
                var frameworkDetectionHelper = ShellInstance.GetComponent<IFrameworkDetectionHelper>();
                CompileCs.Compile(frameworkDetectionHelper, source, dll, references, GetPlatformID().Version);
            }

            return dll.Name;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IEnumerable<string> GetAllSourceFilesInTestDirectory()
        {
            InferProductHomeDir();
            return TestDataPath2.GetChildFiles("*.cs").Select(p => p.Name);
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