using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace tests.xunit.eyeball
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
                Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
                Assert.Equal(expectedLength, value.Length);
            }

            // TEST: Ensure all 3 tests pass
            [Theory]
            [InlineData("hello", 5)]
            [InlineData("hello world", 11)]
            [InlineData("cheese", 6)]
            public void SuccessfulMultipleLineTheoryTest(string value, int expectedLength)
            {
                Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
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
            [InlineData(1)]
            [InlineData(2)]
            [InlineData(3)]
            [InlineData(4)]
            [InlineData(5)]
            public void EachTheoryThrowsAnException(int value)
            {
                throw new Exception(string.Format("Exception no: {0}", value));
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

            [Theory]
            [PropertyData("RandomDataEnumerator")]
            public void WithRandomData(int value)
            {
                Console.WriteLine("Hello: {0}", value);
            }

            public static IEnumerable<object[]> RandomDataEnumerator
            {
                get
                {
                    var random = new Random(Environment.TickCount);
                    var min = random.Next(1000);
                    var max = random.Next(20) + min;
                    Console.WriteLine("Running tests from {0} to {1}", min, max);
                    return Enumerable.Range(min, max).Select(x => new object[] {x});
                }
            }
        }
    }
}
