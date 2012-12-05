using System.Collections.Generic;
using System.Linq;
using Xunit.Extensions;

namespace tests.xunit.eyeball.propertydata
{
    public class PropertyDataReferenceInOtherClass
    {
        // TEST: Should use ProvidesPropertyData.TheoryDataEnumerator
        // TEST: Should be able to ctrl-click navigate
        // TEST: Should show this line in find usages on ProvidesPropertyData.TheoryDataEnumerator
        [Theory]
        [PropertyData("TheoryDataEnumerator", PropertyType = typeof (ProvidesPropertyData))]
        public void Test(int value)
        {
        }
    }

    public class ProvidesPropertyData
    {
        // TEST: Should be marked as in use
        public static IEnumerable<object[]> TheoryDataEnumerator
        {
            get { return Enumerable.Range(1, 10).Select(x => new object[] { x }); }
        }

        // TEST: Should be marked as not in use
        public static IEnumerable<object[]> UnusedTheoryDataEnumerator
        {
            get { return Enumerable.Range(1, 10).Select(x => new object[] { x }); }
        }
    }
}