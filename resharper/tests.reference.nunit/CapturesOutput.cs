using System;
using System.Diagnostics;
using NUnit.Framework;

namespace tests.reference.nunit
{
    namespace CapturesOutput
    {
        [TestFixture]
        public class CapturesStandardOutput
        {
            [Test]
            public void CapturesStdOut()
            {
                Console.WriteLine("Hello from stdout");
            }

            [Test]
            public void CapturesStdErr()
            {
                Console.Error.WriteLine("Hello from stderr");
            }
        }

        [TestFixture]
        public class CapturesDebugTrace
        {
            [Test]
            public void CapturesDebugTraceOutput()
            {
                Debug.WriteLine("Hello from Debug.WriteLine");
            }

            [Test]
            public void CapturesTraceOutput()
            {
                Trace.WriteLine("Hello from Trace.WriteLine");
            }

            [Test]
            public void CapturesTraceInformationOutput()
            {
                Trace.TraceInformation("Hello from Trace.TraceInformation");
            }

            [Test]
            public void CapturesTraceWarningOutput()
            {
                Trace.TraceWarning("Hello from Trace.TraceWarning");
            }

            [Test]
            public void CapturesTraceErrorOutput()
            {
                Trace.TraceError("Hello from Trace.TraceError");
            }
        }
    }
}
