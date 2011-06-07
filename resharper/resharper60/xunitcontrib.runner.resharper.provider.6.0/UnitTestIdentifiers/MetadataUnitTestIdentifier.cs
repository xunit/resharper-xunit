using JetBrains.Metadata.Reader.API;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class MetadataUnitTestIdentifier
    {
        public static bool IsUnitTestContainer(this IMetadataTypeInfo metadataTypeInfo)
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