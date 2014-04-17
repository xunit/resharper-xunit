using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Foo
{
    // Separate classes so we don't hit random ordering issues
    public class TheoryWithConstantValue
    {
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
    }

    public class TheoryWithToStringValue
    {
        public class Data
        {
            public string Value;

            public override string ToString()
            {
                return "AlwaysTheSame.Data";
            }
        }

        // Should produce test names of:
        // DuplicateClassInstanceParameter(value: AlwaysTheSame.Data)
        // DuplicateClassInstanceParameter(value: AlwaysTheSame.Data) [2]
        // DuplicateClassInstanceParameter(value: AlwaysTheSame.Data) [3]
        [Theory]
        [PropertyData("ToStringValues")]
        public void DuplicateToStringValue(Data data)
        {
        }

        public static IEnumerable<object[]> ToStringValues
        {
            get
            {
                yield return new[] { new Data() };
                yield return new[] { new Data() };
                yield return new[] { new Data() };
            }
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
