using System;
using JetBrains.ReSharper.Psi;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class PsiAttributeInfoAdapter : IAttributeInfo
    {
        readonly IAttributeInstance attribute;

        public PsiAttributeInfoAdapter(IAttributeInstance attribute)
        {
            this.attribute = attribute;
        }

        public T GetInstance<T>() where T : Attribute
        {
            return null;
        }

        public TValue GetPropertyValue<TValue>(string propertyName)
        {
            var attributeValue = attribute.NamedParameter(propertyName);
            if (attributeValue.IsConstant)
                return (TValue)attributeValue.ConstantValue.Value;
            return default(TValue);
        }
    }
}