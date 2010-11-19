using System;
using Xunit;

namespace test.xunit_silverlight
{
    public class SanityCheckTests
    {
        [Fact]
        public void ThisTestShouldFail()
        {
            // Make sure the testing framework is capturing failing tests
            throw new Exception("This test should fail!");
        }
    }
}