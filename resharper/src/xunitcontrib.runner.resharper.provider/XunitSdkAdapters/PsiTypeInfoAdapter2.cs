using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class PsiTypeInfoAdapter2 : ITypeInfo
    {
        private readonly ITypeElement typeElement;
        private readonly ISubstitution substitution;
        private readonly IModifiersOwner modifiersOwner;

        // Cannot be a closed generic type
        public PsiTypeInfoAdapter2(ITypeElement typeElement)
            : this(typeElement, typeElement.IdSubstitution)
        {
        }

        // Can be a closed generic type, or just a type
        public PsiTypeInfoAdapter2(ITypeElement typeElement, ISubstitution substitution)
        {
            this.typeElement = typeElement;
            this.substitution = substitution;

            // This can be null, but shouldn't be for the types we're interested in
            // i.e. class, struct, interface, etc.
            modifiersOwner = (IModifiersOwner)typeElement;
            Name = typeElement.GetClrName().FullName;
            if (!substitution.IsIdOrEmpty())
            {
                var names = from typeParameter in substitution.Domain
                    let element = substitution[typeParameter].GetTypeElement<ITypeElement>()
                    where element != null
                    select "[" + element.GetClrName() + GetAssemblyName(element) + "]";
                Name += string.Format("[{0}]", names.Join(","));
            }
        }

        private string GetAssemblyName(ITypeElement element)
        {
            var module = element.Module as IAssemblyPsiModule;
            if (module == null)
                return string.Empty;
            return ", " + module.Assembly.AssemblyName;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var fullName = assemblyQualifiedAttributeTypeName;
            return from attribute in typeElement.GetAttributeInstances(true)
                let attributeType = attribute.GetAttributeType()
                where attributeType.GetClrName().FullName == fullName
                      || attributeType.GetAllSuperTypes().Any(t => t.GetClrName().FullName == fullName)
                select (IAttributeInfo) new PsiAttributeInfoAdapter2(attribute);
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            if (!substitution.IsIdOrEmpty())
            {
                return from typeParameter in typeElement.TypeParameters
                    select (ITypeInfo) new PsiTypeInfoAdapter2(substitution[typeParameter].GetTypeElement<ITypeElement>());
            }
            return from typeParameter in typeElement.TypeParameters
                select (ITypeInfo) new GenericArgumentTypeInfoAdapter(Assembly, typeParameter.ShortName);
        }

        public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
        {
            var methods = from method in GetAllMethods()
                where method.ShortName == methodName && (includePrivateMethod || IsPublic(method))
                select (IMethodInfo) new PsiMethodInfoAdapter2(this, method);

            return methods.FirstOrDefault();
        }

        public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
        {
            return from method in GetAllMethods()
                where includePrivateMethods || IsPublic(method)
                select (IMethodInfo) new PsiMethodInfoAdapter2(this, method);
        }

        public IAssemblyInfo Assembly
        {
            get
            {
                // TODO: Reuse...
                return new PsiAssemblyInfoAdapter(((IProjectPsiModule) typeElement.Module).Project);
            }
        }

        public ITypeInfo BaseType
        {
            get
            {
                var @class = typeElement as IClass;
                if (@class != null)
                {
                    var baseClassType = @class.GetBaseClassType();
                    if (baseClassType == null)
                        return null;
                    return new PsiTypeInfoAdapter2(baseClassType.GetTypeElement(), baseClassType.GetSubstitution());
                }
                return null;
            }
        }

        public IEnumerable<ITypeInfo> Interfaces
        {
            get
            {
                var superTypes = from superType in typeElement.GetAllSuperTypes()
                    let element = superType.GetTypeElement()
                    where element is IInterface
                    select (ITypeInfo) new PsiTypeInfoAdapter2(element, superType.GetSubstitution());
                return superTypes;
            }
        }

        public bool IsAbstract { get { return modifiersOwner.IsAbstract; } }
        public bool IsGenericParameter { get { return false; } }
        public bool IsGenericType { get { return Enumerable.Any(typeElement.TypeParameters); } }
        public bool IsSealed { get { return modifiersOwner.IsSealed; } }
        public bool IsValueType { get { return typeElement is IStruct; } }
        public string Name { get; private set; }

        private static bool IsPublic(ITypeMember member)
        {
            return member.AccessibilityDomain.DomainType == AccessibilityDomain.AccessibilityDomainType.PUBLIC;
        }

        private IEnumerable<IMethod> GetAllMethods()
        {
            return (from m in TypeElementUtil.GetAllMembers(typeElement)
                select m.Member).OfType<IMethod>();
        }
    }

    public class PsiMethodInfoAdapter2 : IMethodInfo
    {
        private readonly IMethod method;

        public PsiMethodInfoAdapter2(ITypeInfo typeInfo, IMethod method)
        {
            Type = typeInfo;
            this.method = method;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IParameterInfo> GetParameters()
        {
            throw new NotImplementedException();
        }

        public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments)
        {
            throw new NotImplementedException();
        }

        public bool IsAbstract { get; private set; }
        public bool IsGenericMethodDefinition { get; private set; }
        public bool IsPublic { get; private set; }
        public bool IsStatic { get; private set; }
        public string Name { get { return method.ShortName; } }
        public ITypeInfo ReturnType { get; private set; }
        public ITypeInfo Type { get; private set; }
    }
}