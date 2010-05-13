using System.Linq;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal sealed class UnitTestElementIdentifier
    {
        private static readonly CLRTypeName PropertyDataAttributeName = new CLRTypeName("Xunit.Extensions.PropertyDataAttribute");

        public static bool IsUnitTestElement(IDeclaredElement element)
        {
            return IsUnitTestClass(element) || IsUnitTestMethod(element) || IsUnitTestProperty(element);
        }

        private static bool IsUnitTestClass(IDeclaredElement element)
        {
            return element is IClass && (IsUnitTestContainer(element) || ContainsUnitTestElement((IClass)element));
        }

        public static bool IsUnitTestContainer(IDeclaredElement element)
        {
            var testClass = element as IClass;
            return testClass != null && TypeUtility.IsTestClass(testClass.AsTypeInfo());
        }

        // Returns true if the given element contains an element that is either a
        // unit test or (more likely) a unit test container (class)
        // (i.e. a nested a class that contains a test class)
        // See the comment to SuppressUnusedXunitTestElements for more info
        private static bool ContainsUnitTestElement(ITypeElement element)
        {
            return element.NestedTypes.Aggregate(false, (current, nestedType) => IsUnitTestElement(nestedType) || current);
        }

        private static bool IsUnitTestMethod(IDeclaredElement element)
        {
            return IsUnitTest(element);
        }

        public static bool IsUnitTest(IDeclaredElement element)
        {
            var testMethod = element as IMethod;
            return testMethod != null && MethodUtility.IsTest(testMethod.AsMethodInfo());
        }

        private static bool IsUnitTestProperty(IDeclaredElement element)
        {
            if (element is IAccessor)
            {
                var accessor = ((IAccessor)element);
                return accessor.Kind == AccessorKind.GETTER && IsTheoryPropertyDataProperty(accessor.OwnerMember);
            }

            return element is IProperty && IsTheoryPropertyDataProperty((IProperty)element);
        }

        private static bool IsTheoryPropertyDataProperty(ITypeMember element)
        {
            if (element.IsStatic && element.GetAccessRights() == AccessRights.PUBLIC)
            {
                // According to msdn, parameters to the constructor are positional parameters, and any
                // public read-write fields are named parameters. The name of the property we're after
                // is not a public field/property, so it's a positional parameter
                var propertyNames = from method in element.GetContainingType().Methods
                                    from attributeInstance in method.GetAttributeInstances(PropertyDataAttributeName, false)
                                    select attributeInstance.PositionParameter(0).ConstantValue.Value as string;
                return propertyNames.Any(name => name == element.ShortName);
            }

            return false;
        }
    }
}