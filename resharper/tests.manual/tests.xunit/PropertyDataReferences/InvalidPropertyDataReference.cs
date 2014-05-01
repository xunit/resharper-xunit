using System;
using Xunit;

namespace tests.xunit.eyeball.propertydata
{
    public class InvalidPropertyDataReference
    {
        // TEST: PropertyData attribute value should be marked invalid
        [Theory(Skip = "Invalid PropertyDataAttribute")]
        [MemberData("InvalidAccessibilityTheoryDataEnumerator")]
        public void UsingInvalidDataProperty(int value)
        {
            throw new InvalidOperationException("Should not run");
        }
    }
}