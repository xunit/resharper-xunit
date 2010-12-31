using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
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

        // Provides an instance of ITypeInfo during file editing
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

            // System.Type.GetMethods returns back (BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            // which means all public instance methods on the class and its base classes, and all static methods
            // on this class only (you need BindingFlags.FlattenHierarchy to get the other static methods).
            // We need to replicate this behaviour (I have no idea about ordering...)
            public IEnumerable<IMethodInfo> GetMethods()
            {
                // IClass.Methods returns only the methods of this class
                var publicStaticMethods = from method in psiType.Methods
                                          where method.IsStatic && method.GetAccessRights() == AccessRights.PUBLIC
                                          select method.AsMethodInfo();

                // Let R#'s TypeElementUtil walk the super class chain - we don't have to worry about circular references, etc...
                var allPublicInstanceMethods = from typeMemberInstance in MiscUtil.GetAllClassMembers(psiType)
                                               let typeMember = typeMemberInstance.Member as IMethod
                                               where typeMember != null && !typeMember.IsStatic && typeMember.GetAccessRights() == AccessRights.PUBLIC
                                               select typeMember.AsMethodInfo();

                return allPublicInstanceMethods.Concat(publicStaticMethods);
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

        // Provides an implemenation of ITypeInfo when exploring a physical assembly's metadata
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
                // This can theoretically cause an infinite loop if the class inherits from itself,
                // but seeing as we're wrapping metadata from a physical assembly, I think it would
                // be very difficult to get into that situation
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