using System.Linq;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class PsiUnitTestIdentifier
    {
        private static readonly ClrTypeName PropertyDataAttributeName = new ClrTypeName("Xunit.Extensions.PropertyDataAttribute");

        public static bool IsAnyUnitTestElement(this IDeclaredElement element)
        {
            return IsDirectUnitTestClass(element as IClass) ||
                   IsContainingUnitTestClass(element as IClass) ||
                   IsUnitTestMethod(element as IMethod) ||
                   IsUnitTestDataProperty(element) ||
                   IsUnitTestClassConstructor(element);
        }

        public static bool IsUnitTest(this IDeclaredElement element)
        {
            return IsUnitTestMethod(element as IMethod);
        }

        public static bool IsUnitTestContainer(this IDeclaredElement element)
        {
            return IsDirectUnitTestClass(element as IClass);
        }

        private static bool IsUnitTestClassConstructor(IDeclaredElement element)
        {
            var constructor = element as IConstructor;
            return constructor != null && constructor.IsDefault && IsUnitTestContainer(constructor.GetContainingType());
        }

        private static bool IsDirectUnitTestClass(IClass @class)
        {
            return @class != null && IsExportedType(@class) && TypeUtility.IsTestClass(@class.AsTypeInfo());
        }

        private static bool IsContainingUnitTestClass(IClass @class)
        {
            return @class != null && IsExportedType(@class) &&
                   @class.NestedTypes.Any(IsAnyUnitTestElement);
        }

        private static bool IsExportedType(IAccessRightsOwner @class)
        {
            return @class.GetAccessRights() == AccessRights.PUBLIC;
        }

        private static bool IsUnitTestMethod(IMethod testMethod)
        {
            if (testMethod == null)
                return false;
            var containingType = testMethod.GetContainingType() as IClass;
            if (containingType == null)
                return false;
            return MethodUtility.IsTest(testMethod.AsMethodInfo(containingType.AsTypeInfo()));
        }

        private static bool IsUnitTestDataProperty(IDeclaredElement element)
        {
            var accessor = element as IAccessor;
            if (accessor != null)
            {
                return accessor.Kind == AccessorKind.GETTER && IsTheoryPropertyDataProperty(accessor.OwnerMember);
            }

            return element is IProperty && IsTheoryPropertyDataProperty((IProperty)element);
        }

        private static bool IsTheoryPropertyDataProperty(ITypeMember element)
        {
            if (element.IsStatic && element.GetAccessRights() == AccessRights.PUBLIC)
            {
                // Make sure there is a containing type. The only example I've seen where this is null
                // is when the C# class is included in the project, but not compiled (i.e. build action
                // is set to None). Not sure if this is a bug or not - on the one hand, it's not compiled
                // so there isn't really a type there, on the other, there's enough info for it to be
                // a method, and that method has to live somewhere. Either way, we don't care. If it's
                // not compiled, it's no use to the test runner
                var containingType = element.GetContainingType();
                if (containingType == null)
                    return false;

                // According to msdn, parameters to the constructor are positional parameters, and any
                // public read-write fields are named parameters. The name of the property we're after
                // is not a public field/property, so it's a positional parameter
                var propertyNames = from method in containingType.Methods
                                    from attributeInstance in method.GetAttributeInstances(PropertyDataAttributeName, false)
                                    select attributeInstance.PositionParameter(0).ConstantValue.Value as string;
                return propertyNames.Any(name => name == element.ShortName);
            }

            return false;
        }
    }
}
