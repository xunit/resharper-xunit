using System.Diagnostics;
using Xunit;

namespace Foo
{
    public class DebugAsserts
    {
        [Fact]
        public void TestMethod()
        {
            // TODO: When run as a normal test, this shows an assert dialog
            // When run as part of the test framework, it doesn't. I have no idea why
            // Also, I think showing a dialog is something that should be fixed in xunit
            // See https://github.com/xunit/xunit/issues/382
            throw new System.Exception();
            Debug.Assert(false, "Should not see this as a dialog");
        }
    }
}
