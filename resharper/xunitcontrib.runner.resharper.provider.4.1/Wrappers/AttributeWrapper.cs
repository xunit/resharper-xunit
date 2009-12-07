using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal static class AttributeWrapper
    {
        internal static IAttributeInfo AsAttributeInfo(this IAttributeInstance attribute)
        {
            return new AttributeInstanceWrapper(attribute);
        }

        internal static IAttributeInfo AsAttributeInfo(this IMetadataCustomAttribute attribute)
        {
            return new MetadataCustomAttributeWrapper(attribute);
        }

        private class AttributeInstanceWrapper : IAttributeInfo
        {
            readonly IAttributeInstance attribute;

            public AttributeInstanceWrapper(IAttributeInstance attribute)
            {
                this.attribute = attribute;
            }

            public T GetInstance<T>() where T : Attribute
            {
                return null;
            }

            public TValue GetPropertyValue<TValue>(string propertyName)
            {
                return (TValue)attribute.NamedParameter(new ParameterName(propertyName)).ConstantValue.Value;
            }

            private class ParameterName : ITypeMember
            {
                readonly string name;

                public ParameterName(string name)
                {
                    this.name = name;
                }

                public IList<IDeclaration> GetDeclarations()
                {
                    throw new NotImplementedException();
                }

                public IList<IDeclaration> GetDeclarationsIn(IProjectFile projectFile)
                {
                    throw new NotImplementedException();
                }

                public ISearchDomain GetAccessibilityDomain()
                {
                    throw new NotImplementedException();
                }

                public DeclaredElementType GetElementType()
                {
                    throw new NotImplementedException();
                }

                public PsiManager GetManager()
                {
                    throw new NotImplementedException();
                }

                public ITypeElement GetContainingType()
                {
                    throw new NotImplementedException();
                }

                public ITypeMember GetContainingTypeMember()
                {
                    throw new NotImplementedException();
                }

                public XmlNode GetXMLDoc(bool inherit)
                {
                    throw new NotImplementedException();
                }

                public XmlNode GetXMLDescriptionSummary(bool inherit)
                {
                    throw new NotImplementedException();
                }

                public bool IsValid()
                {
                    throw new NotImplementedException();
                }

                public bool IsSynthetic()
                {
                    throw new NotImplementedException();
                }

                public IList<IProjectFile> GetProjectFiles()
                {
                    throw new NotImplementedException();
                }

                public bool HasDeclarationsInProjectFile(IProjectFile projectFile)
                {
                    throw new NotImplementedException();
                }

                public string ShortName
                {
                    get { return name; }
                }

                public bool CaseSensistiveName
                {
                    get { throw new NotImplementedException(); }
                }

                public string XMLDocId
                {
                    get { throw new NotImplementedException(); }
                }

                public PsiLanguageType Language
                {
                    get { throw new NotImplementedException(); }
                }

                public IModule Module
                {
                    get { throw new NotImplementedException(); }
                }

                public ISubstitution IdSubstitution
                {
                    get { throw new NotImplementedException(); }
                }

                public IList<IAttributeInstance> GetAttributeInstances(bool inherit)
                {
                    throw new NotImplementedException();
                }

                public IList<IAttributeInstance> GetAttributeInstances(CLRTypeName clrName, bool inherit)
                {
                    throw new NotImplementedException();
                }

                public bool HasAttributeInstance(CLRTypeName clrName, bool inherit)
                {
                    throw new NotImplementedException();
                }

                public AccessRights GetAccessRights()
                {
                    throw new NotImplementedException();
                }

                public bool IsAbstract
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsSealed
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsVirtual
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsOverride
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsStatic
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsReadonly
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsExtern
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsUnsafe
                {
                    get { throw new NotImplementedException(); }
                }

                public bool IsVolatile
                {
                    get { throw new NotImplementedException(); }
                }

                public IList<TypeMemberInstance> GetHiddenMembers()
                {
                    throw new NotImplementedException();
                }

                public AccessibilityDomain AccessibilityDomain
                {
                    get { throw new NotImplementedException(); }
                }

                public MemberHidePolicy HidePolicy
                {
                    get { throw new NotImplementedException(); }
                }
            }
        }

        private class MetadataCustomAttributeWrapper : IAttributeInfo
        {
            readonly IMetadataCustomAttribute attribute;

            public MetadataCustomAttributeWrapper(IMetadataCustomAttribute attribute)
            {
                this.attribute = attribute;
            }

            public T GetInstance<T>() where T : Attribute
            {
                return null;
            }

            public TValue GetPropertyValue<TValue>(string propertyName)
            {
                foreach (IMetadataCustomAttributePropertyInitialization prop in attribute.InitializedProperties)
                    if (prop.Property.Name == propertyName)
                        return (TValue)prop.Value;

                return default(TValue);
            }
        }
    }
}