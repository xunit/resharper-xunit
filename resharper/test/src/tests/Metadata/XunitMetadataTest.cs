using System.Collections.Generic;
using System.Linq;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Metadata
{
    public abstract class XunitMetadataTest : XunitMetdataTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Exploration\" + RelativeTestDataPathSuffix; }
        }

        protected abstract string RelativeTestDataPathSuffix { get; }

        [TestCaseSource("GetAllDllsInTestDirectory")]
        public void TestFile(string filename)
        {
            DoTestSolution(GetTestDataFilePath(filename));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IEnumerable<string> GetAllDllsInTestDirectory()
        {
            var dlls = from source in TestDataPath2.GetChildFiles("*.cs")
                let dll = source.ChangeExtension("dll")
                select new {Source = source, Dll = dll};
            foreach (var dll in dlls)
            {
                if (!dll.Dll.ExistsFile || dll.Source.FileModificationTimeUtc > dll.Dll.FileModificationTimeUtc)
                {
                    var references = GetReferencedAssemblies().Select(GetTestDataFilePath).ToArray();
                    CompileUtil.CompileCs(dll.Source, dll.Dll, references);
                }

                yield return dll.Dll.Name;
            }
        }
    }
}