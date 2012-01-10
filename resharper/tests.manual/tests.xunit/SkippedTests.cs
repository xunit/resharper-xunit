using System;
using Xunit;

namespace tests.xunit
{
    public class SkippedTests
    {
        // TEST: Should be flagged as test method
        // TEST: Should not be run
        // TEST: Should display skip reason
        // TEST: Should display "Ignored: This is the skip reason" as result in tree view
        // TEST: Should display test name in grey
        [Fact(Skip = "This is the skip reason")]
        public void SkippedTestMethod()
        {
            // TEST: Should not throw
            throw new NotImplementedException();
        }
    }
}
