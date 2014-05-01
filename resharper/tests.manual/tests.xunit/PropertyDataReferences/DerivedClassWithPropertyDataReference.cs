using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace tests.xunit.eyeball.propertydata
{
    public class DerivedClassWithPropertyDataReference : PropertyDataBase
    {
        // TEST: Should reference TheoryDataEnumerator in base class
        [Theory]
        [MemberData("TheoryDataEnumerator")]
        public void Test(int value)
        {
        }
    }

    public class PropertyDataBase
    {
        // TEST: This should be marked as in use - and it must be public static
        // TEST: Find usages should navigate to the PropertyData attributes above
        // TODO: Stop ReSharper marking this as can be turned into protected
        // (see XamlUsageAnalyzer.ProcessNameDeclaration)
        public static IEnumerable<object[]> TheoryDataEnumerator
        {
            get { return Enumerable.Range(1, 10).Select(x => new object[] { x }); }
        }
    }
}