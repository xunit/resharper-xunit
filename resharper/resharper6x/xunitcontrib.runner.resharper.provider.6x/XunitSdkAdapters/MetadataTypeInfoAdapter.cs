using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // Provides an implementation of ITypeInfo when exploring a physical assembly's metadata
    internal class MetadataTypeInfoAdapter : ITypeInfo
    {
        readonly IMetadataTypeInfo metadataTypeInfo;

        public MetadataTypeInfoAdapter(IMetadataTypeInfo metadataTypeInfo)
        {
            this.metadataTypeInfo = metadataTypeInfo;
        }

        public bool IsAbstract
        {
            get { return metadataTypeInfo.IsAbstract; }
        }

        public bool IsSealed
        {
            get { return metadataTypeInfo.IsSealed; }
        }

        public Type Type
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
        {
            return from attribute in metadataTypeInfo.CustomAttributes
                   where attributeType.IsAssignableFrom(attribute.UsedConstructor.DeclaringType)
                   select attribute.AsAttributeInfo();
        }

        public IMethodInfo GetMethod(string methodName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMethodInfo> GetMethods()
        {
            // This can theoretically cause an infinite loop if the class inherits from itself,
            // but seeing as we're wrapping metadata from a physical assembly, I think it would
            // be very difficult to get into that situation
            var currentType = metadataTypeInfo;
            do
            {
                foreach (var method in currentType.GetMethods())
                    yield return method.AsMethodInfo();

                currentType = currentType.Base.Type;
            } while (currentType.Base != null);
        }

        public bool HasAttribute(Type attributeType)
        {
            return GetCustomAttributes(attributeType).Any();
        }

        public bool HasInterface(Type attributeType)
        {
            return metadataTypeInfo.Interfaces.Any(implementedInterface => implementedInterface.Type.FullyQualifiedName == attributeType.FullName);
        }

        public override string ToString()
        {
            return metadataTypeInfo.FullyQualifiedName;
        }
    }
}