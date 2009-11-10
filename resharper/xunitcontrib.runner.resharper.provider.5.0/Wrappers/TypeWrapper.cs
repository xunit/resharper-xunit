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
            readonly IClass psiType;

            public PsiClassWrapper(IClass psiClass)
            {
                psiType = psiClass;
            }

            public bool IsAbstract
            {
                get { return psiType.IsAbstract; }
            }

            public bool IsSealed
            {
                get { return psiType.IsSealed; }
            }

            public Type Type
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return psiType.GetAttributeInstances(false).Select(attribute => AttributeWrapper.Wrap(attribute));
            }

            public IMethodInfo GetMethod(string methodName)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IMethodInfo> GetMethods()
            {
                var currentType = psiType;
                do
                {
                    foreach (var method in currentType.Methods)
                        yield return MethodWrapper.Wrap(method);

                    currentType = currentType.GetSuperClass();
                } while (currentType != null);
            }

            public bool HasAttribute(Type attributeType)
            {
                return psiType.GetAttributeInstances(false).Any(attribute => IsType(attribute.AttributeType, attributeType.FullName));
            }

            public bool HasInterface(Type interfaceType)
            {
                return IsType(psiType, interfaceType.FullName);
            }

            public override string ToString()
            {
                return psiType.CLRName;
            }
        }

        class MetadataTypeInfoWrapper : ITypeInfo
        {
            readonly IMetadataTypeInfo metadataTypeInfo;

            public MetadataTypeInfoWrapper(IMetadataTypeInfo metadataTypeInfo)
            {
                this.metadataTypeInfo = metadataTypeInfo;
            }

            public bool IsAbstract
            {
                get { return metadataTypeInfo.IsAbstract; }
            }

            public bool IsSealed
            {
                get { return metadataTypeInfo.IsSealed; }
            }

            public Type Type
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                return metadataTypeInfo.CustomAttributes.Select(attribute => AttributeWrapper.Wrap(attribute));
            }

            public IMethodInfo GetMethod(string methodName)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IMethodInfo> GetMethods()
            {
                var currentType = metadataTypeInfo;
                do
                {
                    foreach (var method in currentType.GetMethods())
                        yield return MethodWrapper.Wrap(method);

                    currentType = currentType.Base.Type;
                } while (currentType.Base != null);
            }

            public bool HasAttribute(Type attributeType)
            {
                foreach (var attribute in metadataTypeInfo.CustomAttributes)
                {
                    if (attribute.UsedConstructor == null)
                        continue;

                    var attributeTypeInfo = attribute.UsedConstructor.DeclaringType;

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
                return metadataTypeInfo.Interfaces.Any(implementedInterface => implementedInterface.Type.FullyQualifiedName == attributeType.FullName);
            }

            public override string ToString()
            {
                return metadataTypeInfo.FullyQualifiedName;
            }
        }
    }
}