using System.Linq;
using JetBrains.Metadata.Reader.API;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class MetadataTypeSystem
    {
        public static ITypeInfo GetType(IMetadataType type, MetadataTypeInfoAdapter2 typeInfo,
            MetadataMethodInfoAdapter2 methodInfo)
        {
            var classType = type as IMetadataClassType;
            if (classType != null)
                return new MetadataTypeInfoAdapter2(classType);

            var argumentReferenceType = type as IMetadataGenericArgumentReferenceType;
            if (argumentReferenceType != null)
                return GetTypeFromGenericArgumentReferenceType(argumentReferenceType, typeInfo, methodInfo);

            return null;
        }

        private static ITypeInfo GetTypeFromGenericArgumentReferenceType(IMetadataGenericArgumentReferenceType reference, ITypeInfo typeInfo, IMethodInfo methodInfo)
        {
            var argument = reference.Argument;
            switch (argument.Kind)
            {
                case GenericArgumentKind.Type:
                    return typeInfo.GetGenericArguments().ToList()[(int)argument.Index];

                case GenericArgumentKind.Method:
                    return methodInfo.GetGenericArguments().ToList()[(int)argument.Index];
            }

            return null;
        }
    }
}