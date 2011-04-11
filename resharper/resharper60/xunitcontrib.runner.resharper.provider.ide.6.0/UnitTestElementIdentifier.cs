using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;
using XunitContrib.Runner.ReSharper.UnitTestRunnerProvider.XunitSdkAdapters;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class UnitTestElementIdentifier
    {
        private static readonly ClrTypeName PropertyDataAttributeName = new ClrTypeName("Xunit.Extensions.PropertyDataAttribute");

        public static bool IsAnyUnitTestElement(IDeclaredElement element)
        {
            return IsDirectUnitTestClass(element as IClass) || IsContainingUnitTestClass(element as IClass) || IsUnitTestMethod(element) || IsUnitTestDataProperty(element);
        }

        public static bool IsUnitTest(IDeclaredElement element)
        {
            return IsUnitTestMethod(element);
        }

        public static bool IsUnitTestContainer(IDeclaredElement element)
        {
            return IsDirectUnitTestClass(element as IClass);
        }

        public static bool IsUnitTestContainer(IMetadataTypeInfo metadataTypeInfo)
        {
            return IsDirectUnitTestClass(metadataTypeInfo);
        }

        public static bool IsUnitTestStuff(IDeclaredElement element)
        {
            return IsContainingUnitTestClass(element as IClass) || IsUnitTestDataProperty(element);
        }


        private static bool IsDirectUnitTestClass(IClass @class)
        {
            return @class != null && IsExportedType(@class) && TypeUtility.IsTestClass(@class.AsTypeInfo());
        }

        private static bool IsDirectUnitTestClass(IMetadataTypeInfo metadataTypeInfo)
        {
            return IsExportedType(metadataTypeInfo) && TypeUtility.IsTestClass(metadataTypeInfo.AsTypeInfo());
        }

        private static bool IsContainingUnitTestClass(IClass @class)
        {
            return @class != null && IsExportedType(@class) &&
                   @class.NestedTypes.Aggregate(false, (foundAnyUnitTestElements, nestedType) => IsAnyUnitTestElement(nestedType) || foundAnyUnitTestElements);
        }

        private static bool IsExportedType(IAccessRightsOwner @class)
        {
            return @class.GetAccessRights() == AccessRights.PUBLIC;
        }

        private static bool IsExportedType(IMetadataTypeInfo metadataTypeInfo)
        {
            return metadataTypeInfo.IsPublic || metadataTypeInfo.IsNestedPublic;
        }

        private static bool IsUnitTestMethod(IDeclaredElement element)
        {
            var testMethod = element as IMethod;
            return testMethod != null && MethodUtility.IsTest(testMethod.AsMethodInfo());
        }

        private static bool IsUnitTestDataProperty(IDeclaredElement element)
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