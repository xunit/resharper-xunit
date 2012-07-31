using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class PsiTypeInfoAdapter : ITypeInfo
    {
        readonly IClass psiType;

        public PsiTypeInfoAdapter(IClass psiType)
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
            var allPublicInstanceMethods = from typeMemberInstance in psiType.GetAllClassMembers()
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
            return psiType.GetClrName().FullName;
        }
    }
}
