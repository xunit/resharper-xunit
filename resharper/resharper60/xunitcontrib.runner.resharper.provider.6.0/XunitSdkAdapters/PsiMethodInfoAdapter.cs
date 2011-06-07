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
        readonly IMethod method;

        public PsiMethodInfoAdapter(IMethod method)
        {
            this.method = method;
        }

        public string TypeName
        {
            get { return method.GetContainingType().GetClrName().FullName; }
        }

        public void Invoke(object testClass, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public ITypeInfo Class
        {
            get { return ((IClass)method.GetContainingType()).AsTypeInfo(); }
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
                   where attributeType.IsAssignableFrom(attribute.AttributeType)
                   select attribute.AsAttributeInfo();
        }

        public bool HasAttribute(Type attributeType)
        {
            return method.GetAttributeInstances(false).Any(attribute => attributeType.IsAssignableFrom(attribute.AttributeType));
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