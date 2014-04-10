using System;
using Xunit;

namespace Foo
{
    public class FailingFact
    {
        [Fact]
        public void Fails()
        {
            Assert.Equal(12, 42);
        }
    }
}
