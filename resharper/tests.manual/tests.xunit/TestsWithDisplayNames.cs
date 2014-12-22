using System;
using Xunit;

namespace tests.xunit.unsupported
{
    namespace ExpectedToFail.NotYetImplemented
    {
        public class TestsWithDisplayNames
        {
            // TEST: Name should be reported as attribute value, with no namespaces
            [Fact(DisplayName = "NameComesFromAttribute")]
            public void ShouldNotSeeThis_NameShouldComeFromAttribute()
            {
                //Assert.Equal(1, 1);
                throw new NotSupportedException();
            }

            // TEST: Name should be reported as attribute value, with no namespaces
            [Fact(DisplayName = "Name contains spaces")]
            public void ShouldNotSeeThis_NameShouldContainSpaces()
            {
                //Assert.Equal(1, 1);
                throw new NotSupportedException();
            }

            [Theory(DisplayName = "This is a theory with a display name")]
            [InlineData("cheese")]
            public void ShouldNotSeeThis_NameShouldComeFromTheoryAttribute(string data)
            {
                throw new NotSupportedException();
            }
        }
    }
}
