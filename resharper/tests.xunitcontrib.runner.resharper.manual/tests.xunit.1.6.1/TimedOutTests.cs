using System.Threading;
using Xunit;

namespace tests.xunit
{
    namespace ExpectedToFail
    {
        public class TimedOutTests
        {
            // TEST: Should fail
            // TEST: Should display "Failed: Test execution time exceeded: 100ms"
            // BUG: Displays "-- Exception doesn't have a stack trace"
            [Fact(Timeout = 100)]
            public void TestShouldTimeOut()
            {
                Thread.Sleep(200);
            }
        }
    }
}