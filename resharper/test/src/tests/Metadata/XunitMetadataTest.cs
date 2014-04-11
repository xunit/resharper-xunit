using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.Metadata
{
    public abstract class XunitMetadataTest : XunitMetdataTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            EnvironmentVariables.SetUp(BaseTestDataPath);
        }

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
                var references = GetReferencedAssemblies()
                    .Select(Environment.ExpandEnvironmentVariables).ToArray();
                CompileUtil.CompileCs(source, dll, references, false,
                    false, GetPlatformID().Version.ToString(2));
            }

            return dll.Name;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IEnumerable<string> GetAllSourceFilesInTestDirectory()
        {
            return TestDataPath2.GetChildFiles("*.cs").Select(p => p.Name);
        }
    }
}