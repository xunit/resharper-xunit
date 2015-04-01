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
            return from attribute in psiType.GetAttributeInstances(true)
                   where attributeType.IsAssignableFrom(attribute.GetAttributeType())
                   select attribute.AsAttributeInfo();
        }

        public IMethodInfo GetMethod(string methodName)
        {
            throw new NotImplementedException();
        }

        // xunit's BindingFlags for GetMethods is NonPublic | Public | Instance | Static, which
        // means all methods for this particular class (public, private, instance and static) and
        // also all inherited instance methods (so public and protected methods on the base class
        // - you need to add FlattenHierarchy to get public and protected static methods defined
        // on the base class). You do not get private methods, instance or static, because they
        // are not inherited
        public IEnumerable<IMethodInfo> GetMethods()
        {
            // Get all non-private, non-static instance methods for this type, including
            // inherited methods
            var inheritedInstanceMethods = from typeMemberInstance in psiType.GetAllClassMembers()
                                           let method = typeMemberInstance.Member as IMethod
                                           where method != null && !method.IsStatic && method.GetAccessRights() != AccessRights.PRIVATE
                                           select method.AsMethodInfo(this);

            // Get private or static methods declared only on this type (no inheritance)
            var localStaticOrPublicMethods = from method in psiType.Methods
                                             where method.IsStatic || method.GetAccessRights() == AccessRights.PRIVATE
                                             select method.AsMethodInfo(this);

            return inheritedInstanceMethods.Concat(localStaticOrPublicMethods);
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
