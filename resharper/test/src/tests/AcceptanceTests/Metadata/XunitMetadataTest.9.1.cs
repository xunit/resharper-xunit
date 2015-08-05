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
            TestUtil.SetHomeDir(GetType().Assembly);
        }
    }
}