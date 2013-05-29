using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class PsiMethodInfoAdapter : IMethodInfo
    {
        private IMethod method;

        public PsiMethodInfoAdapter()
        {
        }

        public PsiMethodInfoAdapter(IMethod method, ITypeInfo typeInfo)
        {
            Reset(method, typeInfo);
        }

        public void Reset(IMethod method, ITypeInfo typeInfo)
        {
            this.method = method;
            var containingType = method.GetContainingType();
            TypeName = containingType.GetClrName().FullName;
            Class = typeInfo ?? ((IClass) containingType).AsTypeInfo();
        }

        public string TypeName { get; private set;}

        public void Invoke(object testClass, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public ITypeInfo Class { get; private set;}

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
                var type = method.ReturnType as IDeclaredType;
                return (type == null ? null : type.GetClrName().FullName);
            }
        }

        public object CreateInstance()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
        {
            return from attribute in method.GetAttributeInstances(false)
                   where attributeType.IsAssignableFrom(attribute.GetAttributeType())
                   select attribute.AsAttributeInfo();
        }

        public bool HasAttribute(Type attributeType)
        {
            return method.GetAttributeInstances(false).Any(attribute => attributeType.IsAssignableFrom(attribute.GetAttributeType()));
        }

        public override string ToString()
        {
            var language = method.GetSourceFiles()[0].PrimaryPsiLanguage;
            return string.Format("{0} {1}({2})",
                                 method.ReturnType.GetLongPresentableName(language),
                                 method.ShortName,
                                 string.Join(", ", method.Parameters.Select(param => param.Type.GetLongPresentableName(language)).ToArray()));
        }
    }
}