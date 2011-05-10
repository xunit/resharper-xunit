using JetBrains.Metadata.Reader.API;
using Xunit.Sdk;
using XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.XunitSdkAdapters;

namespace XunitContrib.Runner.ReSharper.UnitTestRunnerProvider
{
    internal static class UnitTestElementIdentifier
    {
        public static bool IsUnitTestContainer(IMetadataTypeInfo metadataTypeInfo)
        {
            return IsExportedType(metadataTypeInfo) && IsTestClass(metadataTypeInfo);
        }

        private static bool IsExportedType(IMetadataTypeInfo metadataTypeInfo)
        {
            return metadataTypeInfo.IsPublic || metadataTypeInfo.IsNestedPublic;
        }

        private static bool IsTestClass(IMetadataTypeInfo metadataTypeInfo)
        {
            return TypeUtility.IsTestClass(metadataTypeInfo.AsTypeInfo());
        }
    }
}