using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class MetadataAttributeInfoAdapter2 : IAttributeInfo
    {
        private readonly IMetadataCustomAttribute attribute;

        public MetadataAttributeInfoAdapter2(IMetadataCustomAttribute attribute)
        {
            this.attribute = attribute;
        }

        public IEnumerable<object> GetConstructorArguments()
        {
            return attribute.ConstructorArguments.Select(arg => arg.Value);
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var attributeType = attribute.UsedConstructor.DeclaringType;
            foreach (var a in attributeType.CustomAttributes)
            {
                var type = a.UsedConstructor.DeclaringType;
                while (type != null)
                {
                    if (type.AssemblyQualifiedName == assemblyQualifiedAttributeTypeName)
                        yield return new MetadataAttributeInfoAdapter2(a);
                    type = type.Base != null ? type.Base.Type : null;
                }
            }
        }

        public TValue GetNamedArgument<TValue>(string argumentName)
        {
            foreach (var initialization in attribute.InitializedProperties)
            {
                if (initialization.Property.Name == argumentName)
                    return (TValue) initialization.Value.Value;
            }
            foreach (var initialization in attribute.InitializedFields)
            {
                if (initialization.Field.Name == argumentName)
                    return (TValue) initialization.Value.Value;
            }

            return default(TValue);
        }
    }
}