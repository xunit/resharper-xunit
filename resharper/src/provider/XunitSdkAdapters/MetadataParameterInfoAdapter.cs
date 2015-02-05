using JetBrains.Metadata.Reader.API;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class MetadataParameterInfoAdapter : IParameterInfo
    {
        private readonly MetadataTypeInfoAdapter2 typeInfo;
        private readonly MetadataMethodInfoAdapter2 methodInfo;
        private readonly IMetadataParameter parameter;

        public MetadataParameterInfoAdapter(MetadataTypeInfoAdapter2 typeInfo, 
            MetadataMethodInfoAdapter2 methodInfo, IMetadataParameter parameter)
        {
            this.typeInfo = typeInfo;
            this.methodInfo = methodInfo;
            this.parameter = parameter;
        }

        public string Name { get { return parameter.Name; } }

        public ITypeInfo ParameterType
        {
            get { return MetadataTypeSystem.GetType(parameter.Type, typeInfo, methodInfo); }
        }
    }
}