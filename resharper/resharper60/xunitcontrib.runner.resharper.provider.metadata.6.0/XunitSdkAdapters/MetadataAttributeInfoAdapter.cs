using System;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.XunitSdkAdapters
{
    internal class MetadataAttributeInfoAdapter : IAttributeInfo
    {
        readonly IMetadataCustomAttribute attribute;

        public MetadataAttributeInfoAdapter(IMetadataCustomAttribute attribute)
        {
            this.attribute = attribute;
        }

        public T GetInstance<T>() where T : Attribute
        {
            return null;
        }

        public TValue GetPropertyValue<TValue>(string propertyName)
        {
            var values = from property in attribute.InitializedProperties
                         where property.Property.Name == propertyName
                         select (TValue)property.Value.Value;
            return values.FirstOrDefault();
        }
    }
}