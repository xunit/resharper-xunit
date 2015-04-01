using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace tests.reference.nunit
{
    namespace RowTests
    {
        [TestFixture]
        public class RowTests
        {
            // TEST: Ensure test passes
            [Test]
            [TestCase("hello", 5)]
            public void SuccessfulSingleRowTest(string value, int expectedLength)
            {
                Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
                Assert.AreEqual(expectedLength, value.Length);
            }

            // TEST: Ensure all 3 tests pass
            [Test]
            [TestCase("hello", 5)]
            [TestCase("hello world", 11)]
            [TestCase("cheese", 6)]
            public void SuccessfulMultipleRowsTest(string value, int expectedLength)
            {
                Console.WriteLine("String: " + value + " with expected length of " + expectedLength);
                Assert.AreEqual(expectedLength, value.Length);
            }

            [Test]
            [TestCaseSource("SimpleDataEnumerator")]
            public void DataFromProperty(int value)
            {
                Console.WriteLine("Hello: {0}", value);
            }

            public static IEnumerable<int> SimpleDataEnumerator
            {
                get { return Enumerable.Range(1, 10).Select(x => x); }
            }

            [Test]
            [TestCaseSource("RandomDataEnumerator")]
            public void WithRandomData(int value)
            {
                Console.WriteLine("Hello: {0}", value);
            }

            public static IEnumerable<int> RandomDataEnumerator
            {
                get
                {
                    var random = new Random(Environment.TickCount);
                    var start = random.Next(1000);
                    var count = random.Next(20);
                    return Enumerable.Range(start, count).Select(x => x);
                }
            }
        }
    }
}