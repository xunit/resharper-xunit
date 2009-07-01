using System;
using System.Diagnostics;
using Xunit;

namespace tests.xunit
{
    namespace CapturesOutput
    {
        public class CapturesStandardOutput
        {
            // TEST: Runner should display message
            [Fact]
            public void CapturesStdOut()
            {
                Console.WriteLine("Hello from stdout");
            }

            // TEST: Runner should display message
            [Fact]
            public void CapturesStdErr()
            {
                Console.Error.WriteLine("Hello from stderr");
            }
        }

        public class CapturesDebugTrace
        {
            // TEST: Runner should display message
            [Fact(Skip = "xunit does not support capturing output from System.Diagnostics.Trace")]
            public void CapturesDebugTraceOutput()
            {
                Debug.WriteLine("Hello from Debug.WriteLine");
            }

            // TEST: Runner should display message
            [Fact(Skip = "xunit does not support capturing output from System.Diagnostics.Trace")]
            public void CapturesTraceOutput()
            {
                Trace.WriteLine("Hello from Trace.WriteLine");
            }

            // TEST: Runner should display message
            [Fact(Skip = "xunit does not support capturing output from System.Diagnostics.Trace")]
            public void CapturesTraceInformationOutput()
            {
                Trace.TraceInformation("Hello from Trace.TraceInformation");
            }

            // TEST: Runner should display message
            [Fact(Skip = "xunit does not support capturing output from System.Diagnostics.Trace")]
            public void CapturesTraceWarningOutput()
            {
                Trace.TraceWarning("Hello from Trace.TraceWarning");
            }

            // TEST: Runner should display message
            [Fact(Skip = "xunit does not support capturing output from System.Diagnostics.Trace")]
            public void CapturesTraceErrorOutput()
            {
                Trace.TraceError("Hello from Trace.TraceError");
            }
        }
    }
}