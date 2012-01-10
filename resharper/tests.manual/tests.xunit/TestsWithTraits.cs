using System;
using Xunit;

namespace tests.xunit
{
    namespace ExpectedToFail.NotYetImplemented
    {
        public class TestsWithTraits
        {
            [Fact, Trait("name", "value")]
            public void TestHasNameTrait()
            {
                throw new NotImplementedException("What should this do?");
            }

            [Fact, Trait("category", "fancyCategory")]
            public void TestMethodHasCategoryTrait()
            {
                throw new NotImplementedException("Categories not yet supported");
            }
        }
    }
}
