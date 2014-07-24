using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    public class PsiAttributeInfoAdapter2 : IAttributeInfo
    {
        private readonly IAttributeInstance attributeInstance;

        public PsiAttributeInfoAdapter2(IAttributeInstance attributeInstance)
        {
            this.attributeInstance = attributeInstance;
        }

        public IEnumerable<object> GetConstructorArguments()
        {
            if (attributeInstance.Constructor == null)
                yield break;

            foreach (var parameter in attributeInstance.PositionParameters())
            {
                if (parameter.IsConstant && !parameter.IsBadValue)
                    yield return parameter.ConstantValue.Value;
                else
                    yield return null;
            }
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            var typeElement = attributeInstance.GetAttributeType().GetTypeElement();
            if (typeElement == null)
                return EmptyArray<IAttributeInfo>.Instance;

            return from a in typeElement.GetAttributeInstances(true)
                select (IAttributeInfo) new PsiAttributeInfoAdapter2(a);
        }

        public TValue GetNamedArgument<TValue>(string argumentName)
        {
            var attributeValue = attributeInstance.NamedParameter(argumentName);
            if (attributeValue.IsConstant && !attributeValue.IsBadValue)
                return (TValue) attributeValue.ConstantValue.Value;
            return default(TValue);
        }
    }
}