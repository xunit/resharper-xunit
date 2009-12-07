using System;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class TypeExtensions
    {
        internal static bool IsAssignableFrom(this Type type, ITypeElement c)
        {
            return type.FullName == c.CLRName || c.GetSuperTypes().Any(superType => type.IsAssignableFrom(superType));
        }

        internal static bool IsAssignableFrom(this Type type, IDeclaredType c)
        {
            return type.FullName == c.GetCLRName() || c.GetSuperTypes().Any(superType => type.IsAssignableFrom(superType));
        }

        internal static bool IsAssignableFrom(this Type type, IMetadataTypeInfo c)
        {
            return type.FullName == c.FullyQualifiedName || (c.Base != null && type.IsAssignableFrom(c.Base.Type));
        }
    }
}


