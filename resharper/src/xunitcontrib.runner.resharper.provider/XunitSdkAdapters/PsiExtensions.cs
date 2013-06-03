using System;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class PsiExtensions
    {
        internal static ITypeInfo AsTypeInfo(this IClass type)
        {
            return new PsiTypeInfoAdapter(type);
        }

        public static IMethodInfo AsMethodInfo(this IMethod method, ITypeInfo typeInfo)
        {
            return new PsiMethodInfoAdapter(method, typeInfo);
        }

        internal static IAttributeInfo AsAttributeInfo(this IAttributeInstance attribute)
        {
            return new PsiAttributeInfoAdapter(attribute);
        }


        // I always have to think about what this means...
        // Based on Type.IsAssignableFrom(c)
        // "Determines whether an instance of the current Type can be assigned from an instance of the specified Type"
        // True if c and the current type are the same type, or if the current type is in the inheritance hierarchy of
        // the type c
        // In other words, type.IsAssignableFrom(c) is true if I can assign an instance of c to a variable of type "type"
        internal static bool IsAssignableFrom(this Type type, ITypeElement c)
        {
            return type.FullName == c.GetClrName().FullName || c.GetAllSuperTypes().Any(superType => type.FullName == c.GetClrName().FullName);
        }

        internal static bool IsAssignableFrom(this Type type, IDeclaredType c)
        {
            return type.FullName == c.GetClrName().FullName || c.GetAllSuperTypes().Any(superType => type.FullName == superType.GetClrName().FullName);
        }
    }
}


