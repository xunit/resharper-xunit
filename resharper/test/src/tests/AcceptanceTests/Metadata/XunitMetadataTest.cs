using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Components;
using JetBrains.Application.platforms;
using JetBrains.ProjectModel.impl;
using JetBrains.ReSharper.TestFramework;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Metadata
{
    [TestNetFramework4]
    [Category("Metadata discovery")]
    public abstract partial class XunitMetadataTest : XunitMetdataTestBase
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

        partial void InferProductHomeDir();
    }
}