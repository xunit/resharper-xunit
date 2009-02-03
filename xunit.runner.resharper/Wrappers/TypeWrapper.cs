using System;
using System.Collections.Generic;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace Xunit.Runner.ReSharper
{
    static class TypeWrapper
    {
        public static bool IsType(ITypeElement type,
                                  string clrName)
        {
            if (type.CLRName == clrName)
                return true;

            foreach (IDeclaredType superType in type.GetSuperTypes())
                if (IsType(superType, clrName))
                    return true;

            return false;
        }

        public static bool IsType(IDeclaredType type,
                                  string clrName)
        {
            if (type.GetCLRName() == clrName)
                return true;

            foreach (IDeclaredType superType in type.GetSuperTypes())
                if (IsType(superType, clrName))
                    return true;

            return false;
        }

        public static ITypeInfo Wrap(IClass type)
        {
            return new _ClassWrapper(type);
        }

        public static ITypeInfo Wrap(IMetadataTypeInfo type)
        {
            return new _MetadataTypeInfoWrapper(type);
        }

        class _ClassWrapper : ITypeInfo
        {
            readonly IClass type;

            public _ClassWrapper(IClass type)
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
                foreach (IAttributeInstance attribute in type.GetAttributeInstances(false))
                    yield return AttributeWrapper.Wrap(attribute);
            }

            public IEnumerable<IMethodInfo> GetMethods()
            {
                foreach (IMethod method in type.Methods)
                    yield return MethodWrapper.Wrap(method);
            }

            public bool HasAttribute(Type attributeType)
            {
                foreach (IAttributeInstance attribute in type.GetAttributeInstances(false))
                    if (IsType(attribute.AttributeType, attributeType.FullName))
                        return true;

                return false;
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

        class _MetadataTypeInfoWrapper : ITypeInfo
        {
            readonly IMetadataTypeInfo type;

            public _MetadataTypeInfoWrapper(IMetadataTypeInfo type)
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
                foreach (IMetadataCustomAttribute attribute in type.CustomAttributes)
                    yield return AttributeWrapper.Wrap(attribute);
            }

            public IEnumerable<IMethodInfo> GetMethods()
            {
                foreach (IMetadataMethod method in type.GetMethods())
                    yield return MethodWrapper.Wrap(method);
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
                foreach (IMetadataClassType implementedInterface in type.Interfaces)
                    if (implementedInterface.Type.FullyQualifiedName == attributeType.FullName)
                        return true;

                return false;
            }

            public override string ToString()
            {
                return type.FullyQualifiedName;
            }
        }
    }
}