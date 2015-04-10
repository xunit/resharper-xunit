using System;
using JetBrains.Application.BuildScript.Solution;
using JetBrains.TestFramework.Utils;
using JetBrains.Util;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Metadata
{
    public partial class XunitMetadataTest
    {
        partial void InferProductHomeDir()
        {
            // TestCases is called before the environment fixture is run, which would either 
            // ensure Product.Root exists, or infer %JetProductHomeDir%. By using SoutionItemsBasePath 
            // in TestCases, we fallback to the non-extension friendly implementation that expects 
            // the source to be laid out as if we're building the product, not an extension, and it 
            // requires Product.Root. We'll infer it instead. 
            var productHomeDir =
                Environment.GetEnvironmentVariable(AllAssembliesLocator.ProductHomeDirEnvironmentVariableName);
            if (string.IsNullOrEmpty(productHomeDir))
            {
                var assembly = GetType().Assembly;
                var productBinariesDir = assembly.GetPath().Parent;
                if (AllAssembliesLocator.TryGetProductHomeDirOnSources(productBinariesDir).IsNullOrEmpty())
                {
                    var markerPath = TestUtil.GetTestDataPathBase_Find_FromAttr(assembly) ??
                                     TestUtil.DefaultTestDataPath;
                    var directoryNameOfItemAbove = FileSystemUtil.GetDirectoryNameOfItemAbove(productBinariesDir,
                        markerPath);
                    Environment.SetEnvironmentVariable(AllAssembliesLocator.ProductHomeDirEnvironmentVariableName,
                        directoryNameOfItemAbove.FullPath);
                }
            }
        }
    }
}