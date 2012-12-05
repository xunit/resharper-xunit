using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Extensions;

namespace tests.xunit.eyeball.propertydata
{
    public class PropertyDataReferences
    {
        // TEST: TheoryDataEnumerator should be highlighted correctly
        // TEST: Intellisense should work in PropertyData, including TheoryDataEnumerator and DerivedReturnTypeTheoryDataEnumerator
        [Theory]
        [PropertyData("TheoryDataEnumerator")]
        public void DataFromProperty(int value)
        {
            Console.WriteLine("DataFromProperty({0})", value);
        }

        [Theory]
        [PropertyData("DerivedReturnTypeTheoryDataEnumerator")]
        public void DataFromProperty2(int value)
        {
            Console.WriteLine("DataFromProperty({0})", value);
        }

        // TEST: This should be marked as in use - and it must be public static
        // TEST: Find usages should navigate to the PropertyData attributes above
        public static IEnumerable<object[]> TheoryDataEnumerator
        {
            get { return Enumerable.Range(1, 10).Select(x => new object[] { x }); }
        }

        // TEST: Should be marked as not in use
        // TEST: Should appear in intelliense - note the different return value
        public static IList<object[]> DerivedReturnTypeTheoryDataEnumerator
        {
            get { return Enumerable.Range(1, 10).Select(x => new object[] { x }).ToList(); }
        }

        // TEST: Should be marked as not in use
        // TEST: Should not be included in intellisense for PropertyData
        private static IEnumerable<object[]> InvalidAccessibilityTheoryDataEnumerator
        {
            get { return Enumerable.Range(1, 10).Select(x => new object[] { x }); }
        }

        // TEST: Should be marked as not in use
        // TEST: Should not be included in intellisense for PropertyData
        public IEnumerable<object[]> InvalidNonStaticTheoryDataEnumerator
        {
            get { return Enumerable.Range(1, 10).Select(x => new object[] { x }); }
        }
    }
}