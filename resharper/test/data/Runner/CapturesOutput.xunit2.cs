using System;
using Xunit;
using Xunit.Abstractions;

namespace Foo
{
    public class CapturesOutput
    {
        private readonly ITestOutputHelper outputHelper;

        public CapturesOutput(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void OutputFromSuccessfulTest()
        {
            outputHelper.WriteLine("This is some output");
        }

        [Fact]
        public void OutputFromFailingTest()
        {
            outputHelper.WriteLine("This is also some output");
            Assert.Equal(12, 42);
        }

        [Fact]
        public void OutputMultipleTimes()
        {
            outputHelper.WriteLine("Line 1");
            outputHelper.WriteLine("Line 2");
            outputHelper.WriteLine("Line 3");
        }
    }
}
