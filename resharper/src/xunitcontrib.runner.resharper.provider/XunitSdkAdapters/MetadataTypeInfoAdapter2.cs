using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Utils;
using JetBrains.Util;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    // TODO: Cache and reuse objects
    public class MetadataTypeInfoAdapter2 : ITypeInfo
    {
        // IMetadatTypeInfo is an open generic, IMetadataClassType is an instance
        // of IMetadataTypeInfo plus substitutions. When querying all the types in
        // an assembly, we can only get IMetadataTypeInfo instances, but any other
        // code could be closed generics, i.e. IMetadataClassType
        [CanBeNull] private readonly IMetadataClassType metadataType;
        private readonly IMetadataTypeInfo metadataTypeInfo;

        // Used by assembly iterating exported types. If it's generic, it's an open generic type
        public MetadataTypeInfoAdapter2(IMetadataTypeInfo metadataTypeInfo)
            : this(null, metadataTypeInfo)
        {
            this.metadataTypeInfo = metadataTypeInfo;
        }

        // Used by assembly getting a type from a fully qualified type name. If it's generic, it's
        // a closed generic type
        public MetadataTypeInfoAdapter2(IMetadataClassType metadataType)
            : this(metadataType, metadataType.Type)
        {
        }

        private MetadataTypeInfoAdapter2(IMetadataClassType metadataType, IMetadataTypeInfo metadataTypeInfo)
        {
//            Assertion.Assert(metadataTypeInfo.IsResolved, "Unresolved type: {0}", metadataType);
            Assertion.Assert(metadataTypeInfo != null, "metadataTypeInfo cannot be null");

            this.metadataType = metadataType;
            this.metadataTypeInfo = metadataTypeInfo;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var fullName = assemblyQualifiedAttributeTypeName.Substring(0,
                assemblyQualifiedAttributeTypeName.IndexOf(','));
            return from a in GetAllAttributes(fullName)
                select (IAttributeInfo) new MetadataAttributeInfoAdapter2(a);
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            if (metadataType != null && metadataType.Arguments.Length > 0)
            {
                return from t in metadataType.Arguments
                    let type = t as IMetadataClassType
                    select (ITypeInfo) new MetadataTypeInfoAdapter2(type, type.Type);
            }

            return from metadataGenericArgument in metadataTypeInfo.GenericParameters
                select (ITypeInfo) new GenericArgumentTypeInfoAdapter(Assembly, metadataGenericArgument.Name);
        }

        public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
        {
            var methodInfos = from m in GetAllMethods()
                where m.Name == methodName && (includePrivateMethod || m.IsPublic)
                select (IMethodInfo) new MetadataMethodInfoAdapter2(this, m);
            return methodInfos.FirstOrDefault();
        }

        public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
        {
            return from m in GetAllMethods()
                where includePrivateMethods || m.IsPublic
                select (IMethodInfo) new MetadataMethodInfoAdapter2(this, m);
        }

        public IAssemblyInfo Assembly
        {
            get { return new MetadataAssemblyInfoAdapter(metadataTypeInfo.Assembly); }
        }

        public ITypeInfo BaseType
        {
            get
            {
                if (metadataType != null)
                    return new MetadataTypeInfoAdapter2(metadataType.GetBaseType());
                return new MetadataTypeInfoAdapter2(metadataTypeInfo.Base);
            }
        }

        public IEnumerable<ITypeInfo> Interfaces
        {
            get
            {
                var interfaces = metadataType != null ? metadataType.GetInterfaces() : metadataTypeInfo.Interfaces;
                return interfaces.Select(i => (ITypeInfo) new MetadataTypeInfoAdapter2(i));
            }
        }

        public bool IsAbstract { get { return metadataTypeInfo.IsAbstract; } }
        public bool IsGenericParameter { get { return false; } }
        public bool IsGenericType { get { return metadataTypeInfo.GenericParameters.Length > 0; } }
        public bool IsSealed { get { return metadataTypeInfo.IsSealed; } }
        public bool IsValueType { get { return metadataType.IsValueType(); } }

        public string Name
        {
            get { return metadataType != null ? metadataType.FullName : metadataTypeInfo.FullyQualifiedName; }
        }

        private IEnumerable<IMetadataMethod> GetAllMethods()
        {
            var type = metadataTypeInfo;
            do
            {
                foreach (var method in type.GetMethods())
                {
                    yield return method;
                }

                type = type.Base != null ? type.Base.Type : null;
            } while (type != null && type.Name != "System.Object");
        }

        private IEnumerable<IMetadataCustomAttribute> GetAllAttributes(string fullName)
        {
            var type = metadataTypeInfo;
            do
            {
                var attributes = type.GetCustomAttributes(fullName);
                foreach (var attribute in attributes)
                {
                    yield return attribute;
                }

                type = type.Base != null ? type.Base.Type : null;
            } while (type != null && type.Name != "System.Object");
        }
    }
}