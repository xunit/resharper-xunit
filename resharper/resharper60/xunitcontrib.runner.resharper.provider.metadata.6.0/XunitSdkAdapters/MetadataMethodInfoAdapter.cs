using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Metadata.Reader.API;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.XunitSdkAdapters
{
    internal class MetadataMethodInfoAdapter : IMethodInfo
    {
        readonly IMetadataMethod metadataMethod;

        public MetadataMethodInfoAdapter(IMetadataMethod metadataMethod)
        {
            this.metadataMethod = metadataMethod;
        }

        public string TypeName
        {
            get { return metadataMethod.DeclaringType.FullyQualifiedName; }
        }

        public void Invoke(object testClass, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public bool IsAbstract
        {
            get { return metadataMethod.IsAbstract; }
        }

        public bool IsStatic
        {
            get { return metadataMethod.IsStatic; }
        }

        public MethodInfo MethodInfo
        {
            get { return null; }
        }

        public string Name
        {
            get { return metadataMethod.Name; }
        }

        public string ReturnType
        {
            get { return metadataMethod.ReturnValue.Type.AssemblyQualifiedName; }
        }

        public object CreateInstance()
        {
            throw new NotImplementedException();
        }

        // Get any attributes that are of type attributeType, or that are assignable to attributeType
        // (i.e. attributes that are subclasses of attributeType)
        public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
        {
            return from attribute in metadataMethod.CustomAttributes
                   where attributeType.IsAssignableFrom(attribute.UsedConstructor.DeclaringType)
                   select attribute.AsAttributeInfo();
        }

        public bool HasAttribute(Type attributeType)
        {
            return GetCustomAttributes(attributeType).Any();
        }

        public override string ToString()
        {
            return String.Format("{0} {1}({2})",
                                 metadataMethod.ReturnValue.Type.AssemblyQualifiedName,
                                 metadataMethod.Name,
                                 String.Join(", ", metadataMethod.Parameters.Select(param => param.Type.AssemblyQualifiedName).ToArray()));
        }
    }
}