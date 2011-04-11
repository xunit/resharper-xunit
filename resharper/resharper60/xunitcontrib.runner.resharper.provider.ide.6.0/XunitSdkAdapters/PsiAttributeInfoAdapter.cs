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
            return (TValue)attribute.NamedParameter(propertyName).ConstantValue.Value;
        }
    }
}