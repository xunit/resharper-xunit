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

namespace Xunit.Runner.ReSharper
{
    static class AttributeWrapper
    {
        public static IAttributeInfo Wrap(IAttributeInstance attribute)
        {
            return new _AttributeInstanceWrapper(attribute);
        }

        public static IAttributeInfo Wrap(IMetadataCustomAttribute attribute)
        {
            return new _MetadataCustomAttributeWrapper(attribute);
        }

        class _AttributeInstanceWrapper : IAttributeInfo
        {
            readonly IAttributeInstance attribute;

            public _AttributeInstanceWrapper(IAttributeInstance attribute)
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

            class ParameterName : ITypeMember
            {
                readonly string name;

                public ParameterName(string name)
                {
                    this.name = name;
                }

                public IList<IDeclaration> GetDeclarations()
                {
                    throw new System.NotImplementedException();
                }

                public IList<IDeclaration> GetDeclarationsIn(IProjectFile projectFile)
                {
                    throw new System.NotImplementedException();
                }

                public ISearchDomain GetAccessibilityDomain()
                {
                    throw new System.NotImplementedException();
                }

                public DeclaredElementType GetElementType()
                {
                    throw new System.NotImplementedException();
                }

                public PsiManager GetManager()
                {
                    throw new System.NotImplementedException();
                }

                public ITypeElement GetContainingType()
                {
                    throw new System.NotImplementedException();
                }

                public ITypeMember GetContainingTypeMember()
                {
                    throw new System.NotImplementedException();
                }

                public XmlNode GetXMLDoc(bool inherit)
                {
                    throw new System.NotImplementedException();
                }

                public XmlNode GetXMLDescriptionSummary(bool inherit)
                {
                    throw new System.NotImplementedException();
                }

                public bool IsValid()
                {
                    throw new System.NotImplementedException();
                }

                public bool IsSynthetic()
                {
                    throw new System.NotImplementedException();
                }

                public IList<IProjectFile> GetProjectFiles()
                {
                    throw new System.NotImplementedException();
                }

                public bool HasDeclarationsInProjectFile(IProjectFile projectFile)
                {
                    throw new System.NotImplementedException();
                }

                public string ShortName
                {
                    get { return name; }
                }

                public bool CaseSensistiveName
                {
                    get { throw new System.NotImplementedException(); }
                }

                public string XMLDocId
                {
                    get { throw new System.NotImplementedException(); }
                }

                public PsiLanguageType Language
                {
                    get { throw new System.NotImplementedException(); }
                }

                public IModule Module
                {
                    get { throw new System.NotImplementedException(); }
                }

                public ISubstitution IdSubstitution
                {
                    get { throw new System.NotImplementedException(); }
                }

                public IList<IAttributeInstance> GetAttributeInstances(bool inherit)
                {
                    throw new System.NotImplementedException();
                }

                public IList<IAttributeInstance> GetAttributeInstances(CLRTypeName clrName, bool inherit)
                {
                    throw new System.NotImplementedException();
                }

                public bool HasAttributeInstance(CLRTypeName clrName, bool inherit)
                {
                    throw new System.NotImplementedException();
                }

                public AccessRights GetAccessRights()
                {
                    throw new System.NotImplementedException();
                }

                public bool IsAbstract
                {
                    get { throw new System.NotImplementedException(); }
                }

                public bool IsSealed
                {
                    get { throw new System.NotImplementedException(); }
                }

                public bool IsVirtual
                {
                    get { throw new System.NotImplementedException(); }
                }

                public bool IsOverride
                {
                    get { throw new System.NotImplementedException(); }
                }

                public bool IsStatic
                {
                    get { throw new System.NotImplementedException(); }
                }

                public bool IsReadonly
                {
                    get { throw new System.NotImplementedException(); }
                }

                public bool IsExtern
                {
                    get { throw new System.NotImplementedException(); }
                }

                public bool IsUnsafe
                {
                    get { throw new System.NotImplementedException(); }
                }

                public bool IsVolatile
                {
                    get { throw new System.NotImplementedException(); }
                }

                public IList<TypeMemberInstance> GetHiddenMembers()
                {
                    throw new System.NotImplementedException();
                }

                public AccessibilityDomain AccessibilityDomain
                {
                    get { throw new System.NotImplementedException(); }
                }

                public MemberHidePolicy HidePolicy
                {
                    get { throw new System.NotImplementedException(); }
                }
            }
        }

        class _MetadataCustomAttributeWrapper : IAttributeInfo
        {
            readonly IMetadataCustomAttribute attribute;

            public _MetadataCustomAttributeWrapper(IMetadataCustomAttribute attribute)
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