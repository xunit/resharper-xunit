using System;
using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
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
            throw new NotImplementedException();
        }

        public TValue GetNamedArgument<TValue>(string argumentName)
        {
            throw new NotImplementedException();
        }
    }
}