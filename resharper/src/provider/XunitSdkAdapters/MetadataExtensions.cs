using System;
using JetBrains.Metadata.Reader.API;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public static class MetadataExtensions
    {
        public static ITypeInfo AsTypeInfo(this IMetadataTypeInfo type)
        {
            return new MetadataTypeInfoAdapter(type);
        }

        public static IMethodInfo AsMethodInfo(this IMetadataMethod method)
        {
            return new MetadataMethodInfoAdapter(method);
        }

        public static IAttributeInfo AsAttributeInfo(this IMetadataCustomAttribute attribute)
        {
            return new MetadataAttributeInfoAdapter(attribute);
        }

        // I always have to think about what this means...
        // Based on Type.IsAssignableFrom(c)
        // "Determines whether an instance of the current Type can be assigned from an instance of the specified Type"
        // True if c and the current type are the same type, or if the current type is in the inheritance hierarchy of
        // the type c
        // In other words, type.IsAssignableFrom(c) is true if I can assign an instance of c to a variable of type "type"
        // e.g. typeof(IUnitTestProvider).IsAssignableFrom(typeof(UnitTestProvider))
        public static bool IsAssignableFrom(this Type type, IMetadataTypeInfo c)
        {
            // TODO: Can this cause an infinite loop/stack overflow?
            // I think maybe not, because we're dealing with metadata, which means we've successfully compiled
            // and it'll be very hard to compile circular inheritance chains
            return type.FullName == c.FullyQualifiedName || (c.Base != null && type.IsAssignableFrom(c.Base.Type));
        }
    }
}