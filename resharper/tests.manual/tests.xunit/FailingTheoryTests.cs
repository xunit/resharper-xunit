using System;
using Xunit;
using Xunit.Extensions;

namespace tests.xunit.failing
{
    public class FailingTheoryTests
    {
        // TEST: Should run all 3 tests, and should fail for all of them
        [Theory]
        [InlineData("fails", 6)]
        [InlineData("failing", 1)]
        [InlineData("fail as well", 13)]
        public void EachTheoryThrowsAnException(string value, int expectedLength)
        {
            Console.WriteLine("OneFailingTheoryTest(" + value + ") with expected length of " + expectedLength);
            Assert.Equal(expectedLength, value.Length);
        }
        
        // TEST: Should run all 3 tests
        // TEST: "failing" should fail, all others should pass
        [Theory]
        [InlineData("works", 5)]
        [InlineData("failing", 1)]
        [InlineData("works as well", 13)]
        public void OneFailingTheoryTest(string value, int expectedLength)
        {
            Console.WriteLine("OneFailingTheoryTest(" + value + ") with expected length of " + expectedLength);
            Assert.Equal(expectedLength, value.Length);
        }
    }
}