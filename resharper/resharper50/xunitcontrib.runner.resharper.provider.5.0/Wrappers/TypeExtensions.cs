using System;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class TypeExtensions
    {
        // I always have to think about what this means...
        // Based on Type.IsAssignableFrom(c)
        // "Determines whether an instance of the current Type can be assigned from an instance of the specified Type"
        // True if c and the current type are the same type, or if the current type is in the inheritance hierarchy of
        // the type c
        // In other words, type.IsAssignableFrom(c) is true if I can assign an instance of c to a variable of type "type"
        internal static bool IsAssignableFrom(this Type type, ITypeElement c)
        {
            return type.FullName == c.CLRName || TypeElementUtil.GetAllSuperTypes(c).Any(superType => type.FullName == c.CLRName);
        }

        internal static bool IsAssignableFrom(this Type type, IDeclaredType c)
        {
            return type.FullName == c.GetCLRName() || TypeElementUtil.GetAllSuperTypes(c).Any(superType => type.FullName == superType.GetCLRName());
        }

        internal static bool IsAssignableFrom(this Type type, IMetadataTypeInfo c)
        {
            // TODO: Can this cause an infinite loop/stack overflow?
            // I think maybe not, because we're dealing with metadata, which means we've successfully compiled
            // and it'll be very hard to compile circular inheritance chains
            return type.FullName == c.FullyQualifiedName || (c.Base != null && type.IsAssignableFrom(c.Base.Type));
        }
    }
}


