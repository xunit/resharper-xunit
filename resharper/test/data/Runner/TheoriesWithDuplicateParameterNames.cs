using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class SinglePassingFact
    {
        public class Data
        {
            public string Value;

            public override string ToString()
            {
                return "AlwaysTheSame.Data";
            }
        }
        
        public class CustomDataAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
            {
                yield return new[] { new Data() };
                yield return new[] { new Data() };
                yield return new[] { new Data() };
            }
        }

        // Should produce test names of:
        // DuplicateConstantValue(value: 12)
        // DuplicateConstantValue(value: 12) [2]
        // DuplicateConstantValue(value: 12) [3]
        // DuplicateConstantValue(value: 12) [4]
        [Theory]
        [InlineData(12)]
        [InlineData(12)]
        [InlineData(12)]
        [InlineData(12)]
        public void DuplicateConstantValue(int value)
        {
        }

        [Theory]
        [CustomData]
        public void DuplicateClassInstanceParameter(Data data)
        {
        }
    }
}
