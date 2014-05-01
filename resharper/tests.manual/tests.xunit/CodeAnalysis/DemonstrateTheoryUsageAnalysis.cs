using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace tests.xunit.eyeball.sourcecode
{
    public class DemonstrateTheoryUsageAnalysis
    {
        [Theory]
        [MemberData("TheoryDataEnumerator")]
        public void DataFromProperty(int value)
        {
            Console.WriteLine("DataFromProperty({0})", value);
        }

        // TEST: This should be marked as in use - and it must be public static
        public static IEnumerable<object[]> TheoryDataEnumerator
        {
            get { return Enumerable.Range(1, 10).Select(x => new object[] { x }); }
        }
    }
}