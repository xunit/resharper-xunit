using System;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class Theory
    {
        [Theory]
        [InlineData(42)]
        public void SuccessfulMethod(int value)
        {
        }
    }

    public class FailingTheory
    {
        [Theory]
        [InlineData(42)]
        public void FailingMethod(int value)
        {
            throw new InvalidOperationException("Broken");
        }
    }
}

// xunit2 doesn't define Xunit.Extensions
namespace Xunit.Extensions
{
}
