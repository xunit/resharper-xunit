using System;
using System.Threading;
using Xunit;

namespace test.xunitcontrib.runner.silverlight.toolkit.manual
{
    public class ExpectedFailingTests
    {
        [Fact(Timeout = 500)]
        public void ShouldTimeout()
        {
            Thread.Sleep(2000);

            throw new Exception("Expected test to timeout");
        }
    }
}