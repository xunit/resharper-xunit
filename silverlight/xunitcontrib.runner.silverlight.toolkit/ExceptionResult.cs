using System;
using Xunit.Sdk;

namespace XunitContrib.Runner.Silverlight.Toolkit
{
    public class ExceptionResult : FailedResult
    {
        public Exception Exception { get; private set; }

        public ExceptionResult(IMethodInfo method, Exception exception, string displayName)
            : base(method, exception, displayName)
        {
            Exception = exception;
        }
    }
}