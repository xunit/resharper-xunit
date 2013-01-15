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
            // Uh-oh. We're trying to get a property, but ReSharper only gives us
            // access to the constructor args that are set (what the property is
            // set to depends on the code in the constructor). If the constructor
            // is using named args, we're ok, because the name is the property name.
            // If we're not, and we're using positional parameters, we pretty much
            // have to guess. We can make a reasonable assumption that the parameter
            // name is going to match the property name, but probably in a different
            // case. This isn't ideal, but it's the only way we're going to get
            // Traits working
            var attributeValue = attribute.NamedParameter(propertyName);
            if (attributeValue.IsBadValue && attribute.Constructor != null)
            {
                var parameters = attribute.Constructor.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (string.Compare(parameters[i].ShortName, propertyName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        attributeValue = attribute.PositionParameter(i);
                        break;
                    }
                }
            }
            if (attributeValue.IsConstant)
                return (TValue)attributeValue.ConstantValue.Value;
            return default(TValue);
        }
    }
}