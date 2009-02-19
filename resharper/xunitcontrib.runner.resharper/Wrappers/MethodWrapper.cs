using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace Xunit.Runner.ReSharper
{
    static class MethodWrapper
    {
        public static IMethodInfo Wrap(IMethod method)
        {
            return new _MethodWrapper(method);
        }

        public static IMethodInfo Wrap(IMetadataMethod method)
        {
            return new _MetadataMethodWrapper(method);
        }

        class _MetadataMethodWrapper : IMethodInfo
        {
            readonly IMetadataMethod method;

            public _MetadataMethodWrapper(IMetadataMethod method)
            {
                this.method = method;
            }

            public string DeclaringTypeName
            {
                get { return method.DeclaringType.FullyQualifiedName; }
            }

            public bool IsAbstract
            {
                get { return method.IsAbstract; }
            }

            public bool IsStatic
            {
                get { return method.IsStatic; }
            }

            public MethodInfo MethodInfo
            {
                get { return null; }
            }

            public string Name
            {
                get { return method.Name; }
            }

            public string ReturnType
            {
                get { return method.ReturnValue.Type.PresentableName; }
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                foreach (IMetadataCustomAttribute attribute in method.CustomAttributes)
                    yield return AttributeWrapper.Wrap(attribute);
            }

            public bool HasAttribute(Type attributeType)
            {
                foreach (IMetadataCustomAttribute attribute in method.CustomAttributes)
                {
                    if (attribute == null || attribute.UsedConstructor == null)
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

            public override string ToString()
            {
                List<string> paramTypes = new List<string>();
                foreach (IMetadataParameter param in method.Parameters)
                    paramTypes.Add(param.Type.PresentableName);

                return string.Format("{0} {1}({2})",
                                     method.ReturnValue.Type.PresentableName,
                                     method.Name,
                                     string.Join(", ", paramTypes.ToArray()));
            }
        }

        class _MethodWrapper : IMethodInfo
        {
            readonly IMethod method;

            public _MethodWrapper(IMethod method)
            {
                this.method = method;
            }

            public string DeclaringTypeName
            {
                get { return method.GetContainingType().CLRName; }
            }

            public bool IsAbstract
            {
                get { return method.IsAbstract; }
            }

            public bool IsStatic
            {
                get { return method.IsStatic; }
            }

            public MethodInfo MethodInfo
            {
                get { return null; }
            }

            public string Name
            {
                get { return method.ShortName; }
            }

            public string ReturnType
            {
                get
                {
                    IDeclaredType type = method.ReturnType as IDeclaredType;
                    return (type == null ? null : type.GetCLRName());
                }
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                foreach (IAttributeInstance attribute in method.GetAttributeInstances(false))
                    yield return AttributeWrapper.Wrap(attribute);
            }

            public bool HasAttribute(Type attributeType)
            {
                foreach (IAttributeInstance attribute in method.GetAttributeInstances(false))
                    if (TypeWrapper.IsType(attribute.AttributeType, attributeType.FullName))
                        return true;

                return false;
            }

            public override string ToString()
            {
                PsiLanguageType language = new PsiLanguageType("C#");

                List<string> paramTypes = new List<string>();
                foreach (IParameter param in method.Parameters)
                    paramTypes.Add(param.Type.GetLongPresentableName(language));

                return string.Format("{0} {1}({2})",
                                     method.ReturnType.GetLongPresentableName(language),
                                     method.ShortName,
                                     string.Join(", ", paramTypes.ToArray()));
            }
        }
    }
}