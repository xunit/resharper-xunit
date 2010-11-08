using System;
using System.Collections.Generic;
using System.IO;

namespace Xunit
{
    // Used by ExceptionAndOutputCaptureCommand and xunit.extension's TraceAttribute
    public static class Trace
    {
        private static readonly List<TraceListener> listeners = new List<TraceListener>();

        public static List<TraceListener> Listeners
        {
            get { return listeners; }
        }

        public static void WriteLine(string message)
        {
            foreach (var listener in listeners)
            {
                listener.WriteLine(message);
            }
        }

        public static void Assert(bool condition)
        {
            Assert(condition, string.Empty);
        }

        public static void Assert(bool condition, string message)
        {
            if (condition)
                return;

            foreach (var listener in listeners)
            {
                listener.Fail(message);
            }
        }

        public static void Assert(bool condition, string message, string detailedMessage)
        {
            if (condition)
                return;

            foreach (var listener in listeners)
            {
                listener.Fail(message, detailedMessage);
            }
        }
    }

    public abstract class TraceListener
    {
        public virtual void Fail(string message)
        {
            Fail(message, null);
        }
        public abstract void Fail(string message, string detailMessage);
        public abstract void Write(string message);
        public abstract void WriteLine(string message);
    }

    internal class TextWriterTraceListener : TraceListener
    {
        public TextWriterTraceListener(StringWriter outputWriter)
        {
        }

        public override void Fail(string message)
        {
        }

        public override void Fail(string message, string detailMessage)
        {
        }

        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
        }
    }
}