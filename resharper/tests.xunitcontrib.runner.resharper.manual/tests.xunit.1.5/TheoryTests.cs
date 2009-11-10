using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace tests.xunit
{
    // Note that failing theory tests are in the FailingTests namespace
    namespace SuccessfulTheoryTests
    {
        public class TheoryTests
        {
            // TEST: Ensure test passes
            [Theory]
            [InlineData("hello", 5)]
            public void SuccessfulSingleLineTheoryTest(string value, int expectedLength)
            {
                //Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
                Assert.Equal(expectedLength, value.Length);
            }

            // TEST: Ensure all 3 tests pass
            [Theory]
            [InlineData("hello", 5)]
            [InlineData("hello world", 11)]
            [InlineData("cheese", 6)]
            public void SuccessfulMultipleLineTheoryTest(string value, int expectedLength)
            {
                //Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
                Assert.Equal(expectedLength, value.Length);
            }

            // TEST: Executed out of order
            [Theory]
            [InlineData(1)]
            [InlineData(2)]
            [InlineData(3)]
            [InlineData(4)]
            [InlineData(5)]
            [InlineData(6)]
            [InlineData(7)]
            [InlineData(8)]
            [InlineData(9)]
            [InlineData(10)]
            public void DemonstrateTheoryTestRandomOrdering(int indexOfDataAttribute)
            {
                Thread.Sleep(500);
                Console.WriteLine("DemonstrateTheoryTestRandomOrdering(" + indexOfDataAttribute + ")");
            }

            [Theory]
            [PropertyData("TheoryDataEnumerator")]
            public void DataFromProperty(int value)
            {
                Console.WriteLine("DataFromProperty({0})", value);
            }

            // TEST: This should be marked as in use - and it must be public static
            public static IEnumerable<object[]> TheoryDataEnumerator
            {
                get { return Enumerable.Range(1, 10).Select(x => new object[]{x}); }
            }
        }
    }
}
