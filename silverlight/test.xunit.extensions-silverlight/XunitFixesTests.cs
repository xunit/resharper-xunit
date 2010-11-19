using System;
using System.Reflection;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace test.xunit.extensions_silverlight
{
    public class XunitFixesTests
    {
        [Fact]
        public void ExceptionThrownWhenInvokingTheoryCommandProperlyReported()
        {
            var methodInfo = typeof(TheoryCommandTests.TestMethodCommandClass).GetMethod("ThrowsException");
            var command = new TheoryCommand(Reflector.Wrap(methodInfo), null);

            Exception exception = Record.Exception(() => command.Execute(new TheoryCommandTests.TestMethodCommandClass()));

            // If you get a test failure here, then there's another missing instance of "throw;" where there
            // is a call to RethrowWithNoStackTraceLoss. Specifically, for this test, it's in TheoryCommand.Execute
            // Again, it should look like:
            //
            // catch (TargetInvocationException ex)
            // {
            //     ExceptionUtility.RethrowWithNoStackTraceLoss(ex.InnerException);
            //     throw;  // <---- New line
            // }
            if (exception == null || exception.GetType() != typeof(TargetInvocationException))
                throw new ExceptionNotBeingRethrownException("TheoryCommand.Execute");
        }
    }

    public class ExceptionNotBeingRethrownException : Exception
    {
        public ExceptionNotBeingRethrownException(string where)
            : base(string.Format("Expected TargetInvocationException to be rethrown at {0}", where))
        {
        }
    }
}