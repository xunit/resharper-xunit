using System;
using Xunit;
using Xunit.Abstractions;

namespace Foo
{
    public class CapturesOutputFromTheory
    {
        private readonly ITestOutputHelper output;

        public CapturesOutputFromTheory(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(11)]
        [InlineData(42)]
        public void OutputFromSuccessfulTest(int value)
        {
            output.WriteLine("This is some output");
        }

        [Theory]
        [InlineData(12)]
        [InlineData(42)]
        public void OutputFromFailingTest(int value)
        {
            output.WriteLine("This is also some output");
            Assert.Equal(42, value);
        }
    }
}
