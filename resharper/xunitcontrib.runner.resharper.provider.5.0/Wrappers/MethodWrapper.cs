using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    static class MethodWrapper
    {
        public static IMethodInfo Wrap(IMethod method)
        {
            return new PsiMethodWrapper(method);
        }

        public static IMethodInfo Wrap(IMetadataMethod method)
        {
            return new MetadataMethodWrapper(method);
        }

        class MetadataMethodWrapper : IMethodInfo
        {
            readonly IMetadataMethod method;

            public MetadataMethodWrapper(IMetadataMethod method)
            {
                this.method = method;
            }

            public string TypeName
            {
                get { return method.DeclaringType.FullyQualifiedName; }
            }

            public void Invoke(object testClass, params object[] parameters)
            {
                throw new NotImplementedException();
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
                get { return method.ReturnValue.Type.AssemblyQualifiedName; }
            }

            public object CreateInstance()
            {
                throw new NotImplementedException();
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
                    paramTypes.Add(param.Type.AssemblyQualifiedName);

                return string.Format("{0} {1}({2})",
                                     method.ReturnValue.Type.AssemblyQualifiedName,
                                     method.Name,
                                     string.Join(", ", paramTypes.ToArray()));
            }
        }

        class PsiMethodWrapper : IMethodInfo
        {
            readonly IMethod method;

            public PsiMethodWrapper(IMethod method)
            {
                this.method = method;
            }

            public string TypeName
            {
                get { return method.GetContainingType().CLRName; }
            }

            public void Invoke(object testClass, params object[] parameters)
            {
                throw new NotImplementedException();
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

            public object CreateInstance()
            {
                throw new NotImplementedException();
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