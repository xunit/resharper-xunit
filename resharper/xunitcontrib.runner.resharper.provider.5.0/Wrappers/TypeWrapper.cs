using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    static class TypeWrapper
    {
        private static bool IsType(ITypeElement type, string clrName)
        {
            return type.CLRName == clrName || type.GetSuperTypes().Any(superType => IsType(superType, clrName));
        }

        public static bool IsType(IDeclaredType type, string clrName)
        {
            return type.GetCLRName() == clrName || type.GetSuperTypes().Any(superType => IsType(superType, clrName));
        }

        public static ITypeInfo Wrap(IClass type)
        {
            return new PsiClassWrapper(type);
        }

        public static ITypeInfo Wrap(IMetadataTypeInfo type)
        {
            return new MetadataTypeInfoWrapper(type);
        }

        class PsiClassWrapper : ITypeInfo
        {
            readonly IClass type;

            public PsiClassWrapper(IClass type)
            {
                this.type = type;
            }

            public bool IsAbstract
            {
                get { return type.IsAbstract; }
            }

            public bool IsSealed
            {
                get { return type.IsSealed; }
            }

            public Type Type
            {
                get { return null; }
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return type.GetAttributeInstances(false).Select(attribute => AttributeWrapper.Wrap(attribute));
            }

            public IMethodInfo GetMethod(string methodName)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IMethodInfo> GetMethods()
            {
                IClass currentType = type;
                do
                {
                    foreach (IMethod method in currentType.Methods)
                        yield return MethodWrapper.Wrap(method);

                    currentType = currentType.GetSuperClass();
                } while (currentType != null);
            }

            public bool HasAttribute(Type attributeType)
            {
                return type.GetAttributeInstances(false).Any(attribute => IsType(attribute.AttributeType, attributeType.FullName));
            }

            public bool HasInterface(Type interfaceType)
            {
                return IsType(type, interfaceType.FullName);
            }

            public override string ToString()
            {
                return type.CLRName;
            }
        }

        class MetadataTypeInfoWrapper : ITypeInfo
        {
            readonly IMetadataTypeInfo type;

            public MetadataTypeInfoWrapper(IMetadataTypeInfo type)
            {
                this.type = type;
            }

            public bool IsAbstract
            {
                get { return type.IsAbstract; }
            }

            public bool IsSealed
            {
                get { return type.IsSealed; }
            }

            public Type Type
            {
                get { return null; }
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return type.CustomAttributes.Select(attribute => AttributeWrapper.Wrap(attribute));
            }

            public IMethodInfo GetMethod(string methodName)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IMethodInfo> GetMethods()
            {
                IMetadataTypeInfo currentType = type;
                do
                {
                    foreach (IMetadataMethod method in currentType.GetMethods())
                        yield return MethodWrapper.Wrap(method);

                    currentType = currentType.Base.Type;
                } while (currentType.Base != null);
            }

            public bool HasAttribute(Type attributeType)
            {
                foreach (IMetadataCustomAttribute attribute in type.CustomAttributes)
                {
                    if (attribute.UsedConstructor == null)
                        continue;

                    IMetadataTypeInfo attributeTypeInfo = attribute.UsedConstructor.DeclaringType;

                    while (true)
                    {
                        if (attributeTypeInfo.FullyQualifiedName == attributeType.FullName)
                            return true;
                        if (attributeTypeInfo.Base == null)
                            break;
                        attributeTypeInfo = attributeTypeInfo.Base.Type;
                    }
                }

                return false;
            }

            public bool HasInterface(Type attributeType)
            {
                return type.Interfaces.Any(implementedInterface => implementedInterface.Type.FullyQualifiedName == attributeType.FullName);
            }

            public override string ToString()
            {
                return type.FullyQualifiedName;
            }
        }
    }
}