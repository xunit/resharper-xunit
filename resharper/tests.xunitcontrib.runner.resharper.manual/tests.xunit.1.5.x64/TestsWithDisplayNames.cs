using System;
using Xunit;

namespace tests.xunit
{
    namespace ExpectedToFail.NotYetImplemented
    // namespace TestsWithDisplayNames
    {
        public class TestsWithDisplayNames
        {
            // TEST: Name should be reported as attribute value, with no namespaces
            [Fact(Name = "NameComesFromAttribute")]
            public void ShouldNotSeeThis_NameShouldComeFromAttribute()
            {
                //Assert.Equal(1, 1);
                throw new NotImplementedException();
            }

            // TEST: Name should be reported as attribute value, with no namespaces
            [Fact(Name = "Name contains spaces")]
            public void ShouldNotSeeThis_NameShouldContainSpaces()
            {
                //Assert.Equal(1, 1);
                throw new NotImplementedException();
            }
        }
    }
}
