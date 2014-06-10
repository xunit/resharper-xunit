using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace tests.xunit.passing
{
    namespace TheoryTests
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

            [Theory]
            [MemberData("TheoryDataEnumerator")]
            public void DataFromProperty(int value)
            {
                Console.WriteLine("DataFromProperty({0})", value);
            }

            // TEST: This should be marked as in use - and it must be public static
            public static IEnumerable<object[]> TheoryDataEnumerator
            {
                get { return Enumerable.Range(1, 10).Select(x => new object[] {x}); }
            }

            [Theory]
            [MemberData("RandomDataEnumerator")]
            public void WithRandomData(int value, int count)
            {
                Console.WriteLine("Hello: {0} {1}", value, count);
            }

            public static IEnumerable<object[]> RandomDataEnumerator
            {
                get
                {
                    var random = new Random(Environment.TickCount);
                    var start = random.Next(1000);
                    var count = random.Next(20); // +start; // This usually gives loads of tests, and is SLOWWWW!!
                    return Enumerable.Range(start, count).Select(x => new object[] {x, count});
                }
            }

            [Theory]
            [InlineData("plain text")]
            [InlineData("escape mania: $ \\ \b \f \n \r \t \v")]
            [InlineData("unicode mania: \x78\xF2\x12C")]
            public void EscapedStringsShouldNotCrashRunner(string value)
            {
                Console.WriteLine("Value: {0}", value);
            }
        }

        public class GenericTheoryTests
        {
            [Theory]
            [InlineData(42, typeof(int))]
            [InlineData(21.12, typeof(double))]
            [InlineData("Hello, world!", typeof(string))]
            public void CastingGetTest<T>(T value, Type typeOfValue)
            {
                Assert.Equal(value.GetType(), typeOfValue);
            }
        }
    }
}
