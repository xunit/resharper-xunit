using System;
using Microsoft.Silverlight.Testing;
using Xunit;

namespace test.xunit.silverlight
{
    [Exclusive]
    public class SanityCheckTests
    {
        [Fact]
        [Exclusive]
        public void ThisTestShouldFail()
        {
            // Make sure the testing framework is capturing failing tests
            throw new Exception("This test should fail!");
        }
    }
}