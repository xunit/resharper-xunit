using System;
using Xunit;

namespace tests.xunitversions
{
    public class Facts
    {
        [Fact]
        public void PassingTest()
        {
            Assert.Equal(2, 2);
        }

        [Fact(Skip = "Should be skipped")]
        public void SkippedTest()
        {
            throw new InvalidOperationException("Should not be run");
        }

        [Fact]
        public void FailingTest()
        {
            Assert.Equal(3, 2);
        }
    }
}
