using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Metadata.Reader.API;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class MetadataMethodInfoAdapter : IMethodInfo
    {
        readonly IMetadataMethod method;

        public MetadataMethodInfoAdapter(IMetadataMethod method)
        {
            this.method = method;
        }

        public string TypeName
        {
            get { return method.DeclaringType.FullyQualifiedName; }
        }

        public void Invoke(object testClass, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public ITypeInfo Class
        {
            get { return method.DeclaringType.AsTypeInfo(); }
        }

        public bool IsAbstract
        {
            get { return method.IsAbstract; }
        }

        public bool IsStatic
        {
            get { return method.IsStatic; }
        }

        public MethodInfo MethodInfo
        {
            get { return null; }
        }

        public string Name
        {
            get { return method.Name; }
        }

        public string ReturnType
        {
            get { return method.ReturnValue.Type.AssemblyQualifiedName; }
        }

        public object CreateInstance()
        {
            throw new NotImplementedException();
        }

        // Get any attributes that are of type attributeType, or that are assignable to attributeType
        // (i.e. attributes that are subclasses of attributeType)
        public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
        {
            return from attribute in method.CustomAttributes
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
                                 method.ReturnValue.Type.AssemblyQualifiedName,
                                 method.Name,
                                 String.Join(", ", method.Parameters.Select(param => param.Type.AssemblyQualifiedName).ToArray()));
        }
    }
}