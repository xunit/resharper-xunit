using System;
using Xunit;
using Xunit.Extensions;

namespace Foo
{
    public class SinglePassingFact
    {
        [Theory]
        [InlineData(12)]
        [InlineData(33)]
        public void PassingTheories(int value)
        {
        }

        [Theory]
        [InlineData(12)]
        [InlineData(33)]
        public void PassingAndFailingTheories(int value)
        {
            Assert.Equal(12, value);
        }

        [Theory]
        [InlineData(12)]
        [InlineData(33)]
        public void TheoriesWithOutput(int value)
        {
            Console.WriteLine(value);
        }
    }
}
