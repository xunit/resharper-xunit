using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class TypeWrapper
    {
        internal static ITypeInfo AsTypeInfo(this IClass type)
        {
            return new PsiClassWrapper(type);
        }

        internal static ITypeInfo AsTypeInfo(this IMetadataTypeInfo type)
        {
            return new MetadataTypeInfoWrapper(type);
        }

        private class PsiClassWrapper : ITypeInfo
        {
            readonly IClass psiType;

            public PsiClassWrapper(IClass psiType)
            {
                this.psiType = psiType;
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
                return from attribute in psiType.GetAttributeInstances(false)
                       where attributeType.IsAssignableFrom(attribute.AttributeType)
                       select attribute.AsAttributeInfo();
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
                        yield return method.AsMethodInfo();

                    currentType = currentType.GetSuperClass();
                } while (currentType != null);
            }

            public bool HasAttribute(Type attributeType)
            {
                return GetCustomAttributes(attributeType).Any();
            }

            public bool HasInterface(Type interfaceType)
            {
                return interfaceType.IsAssignableFrom(psiType);
            }

            public override string ToString()
            {
                return psiType.CLRName;
            }
        }

        private class MetadataTypeInfoWrapper : ITypeInfo
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
                return from attribute in metadataTypeInfo.CustomAttributes
                       where attributeType.IsAssignableFrom(attribute.UsedConstructor.DeclaringType)
                       select attribute.AsAttributeInfo();
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
                        yield return method.AsMethodInfo();

                    currentType = currentType.Base.Type;
                } while (currentType.Base != null);
            }

            public bool HasAttribute(Type attributeType)
            {
                return GetCustomAttributes(attributeType).Any();
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