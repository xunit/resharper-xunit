using JetBrains.Metadata.Reader.API;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal interface IMetadataTypeProcessor
    {
        void ProcessTypeInfo(IMetadataTypeInfo metadataTypeInfo);
    }
}