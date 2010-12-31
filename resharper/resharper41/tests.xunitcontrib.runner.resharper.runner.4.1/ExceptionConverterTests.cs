using System;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;
using Xunit;
using Xunit.Sdk;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests
{
    public class ExceptionConverterTests
    {
        [Fact]
        public void ConvertsToSingleException()
        {
            Exception singleException = GenerateSingleException();

            string message = ExceptionUtility.GetMessage(singleException);
            string stackTrace = ExceptionUtility.GetStackTrace(singleException);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(singleException.GetType().FullName, message, stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(1, taskExceptions.Length);
            Assert.Equal(singleException.GetType().FullName, taskExceptions[0].Type);
            Assert.Equal(singleException.Message, taskExceptions[0].Message);
            Assert.Equal(singleException.StackTrace, taskExceptions[0].StackTrace);
        }

        [Fact]
        public void ConvertsNestedExceptions()
        {
            // This will generate 3 nested exceptions
            Exception nestedExceptions = GenerateNestedExceptions();

            string message = ExceptionUtility.GetMessage(nestedExceptions);
            string stackTrace = ExceptionUtility.GetStackTrace(nestedExceptions);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(nestedExceptions.GetType().FullName, message, stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(3, taskExceptions.Length);

            Assert.Equal(nestedExceptions.GetType().FullName, taskExceptions[0].Type);
            Assert.Equal(nestedExceptions.Message, taskExceptions[0].Message);
            Assert.Equal(nestedExceptions.StackTrace, taskExceptions[0].StackTrace);

            Assert.Equal(nestedExceptions.InnerException.GetType().FullName, taskExceptions[1].Type);
            Assert.Equal(nestedExceptions.InnerException.Message, taskExceptions[1].Message);
            Assert.Equal(nestedExceptions.InnerException.StackTrace, taskExceptions[1].StackTrace);

            Assert.Equal(nestedExceptions.InnerException.InnerException.GetType().FullName, taskExceptions[2].Type);
            Assert.Equal(nestedExceptions.InnerException.InnerException.Message, taskExceptions[2].Message);
            Assert.Equal(nestedExceptions.InnerException.InnerException.StackTrace, taskExceptions[2].StackTrace);
        }

        [Fact]
        public void RemovesTargetInvocationExceptionFromHeadOfList()
        {
            Exception nestedExceptions = GenerateTargetInvocationException();

            Assert.IsType<TargetInvocationException>(nestedExceptions);

            string message = ExceptionUtility.GetMessage(nestedExceptions);
            string stackTrace = ExceptionUtility.GetStackTrace(nestedExceptions);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(nestedExceptions.GetType().FullName, message, stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(3, taskExceptions.Length);

            Assert.NotEqual(typeof(TargetInvocationException).FullName, taskExceptions[0].Type);

            nestedExceptions = nestedExceptions.InnerException;

            Assert.Equal(nestedExceptions.GetType().FullName, taskExceptions[0].Type);
            Assert.Equal(nestedExceptions.Message, taskExceptions[0].Message);
            Assert.Equal(nestedExceptions.StackTrace, taskExceptions[0].StackTrace);

            Assert.Equal(nestedExceptions.InnerException.GetType().FullName, taskExceptions[1].Type);
            Assert.Equal(nestedExceptions.InnerException.Message, taskExceptions[1].Message);
            Assert.Equal(nestedExceptions.InnerException.StackTrace, taskExceptions[1].StackTrace);

            Assert.Equal(nestedExceptions.InnerException.InnerException.GetType().FullName, taskExceptions[2].Type);
            Assert.Equal(nestedExceptions.InnerException.InnerException.Message, taskExceptions[2].Message);
            Assert.Equal(nestedExceptions.InnerException.InnerException.StackTrace, taskExceptions[2].StackTrace);
        }

        [Fact]
        public void DoesNotRemoveTargetInvocationExceptionIfOnlyException()
        {
            Exception exception = GenerateSingleTargetInvocationException();

            string message = ExceptionUtility.GetMessage(exception);
            string stackTrace = ExceptionUtility.GetStackTrace(exception);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(exception.GetType().FullName, message, stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(1, taskExceptions.Length);
        }

        [Fact]
        public void SimplifiedMessageIsOutermostExceptionTypeShortNameAndMessage()
        {
            Exception nestedExceptions = GenerateNestedExceptions();

            string message = ExceptionUtility.GetMessage(nestedExceptions);
            string stackTrace = ExceptionUtility.GetStackTrace(nestedExceptions);

            string simplifiedMessage;
            ExceptionConverter.ConvertExceptions(nestedExceptions.GetType().FullName, message, stackTrace, out simplifiedMessage);

            Assert.Equal(nestedExceptions.GetType().Name + ": " + nestedExceptions.Message, simplifiedMessage);
        }

        [Fact]
        public void AssertTypeNameDoesNotStoreTypeNameInMessage()
        {
            Exception assertException = GenerateAssertException();

            string message = ExceptionUtility.GetMessage(assertException);
            string stackTrace = ExceptionUtility.GetStackTrace(assertException);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(assertException.GetType().FullName, message, stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(1, taskExceptions.Length);

            Assert.Equal(assertException.GetType().FullName, taskExceptions[0].Type);
            Assert.Equal(assertException.Message, taskExceptions[0].Message);
            Assert.Equal(assertException.StackTrace, taskExceptions[0].StackTrace);
        }

        [Fact]
        public void CanConvertSingleAfterTestException()
        {
            var afterExceptions = new Exception[1];
            afterExceptions[0] = GenerateSingleException();
            var exception = new AfterTestException(afterExceptions);

            string message = ExceptionUtility.GetMessage(exception);
            string stackTrace = ExceptionUtility.GetStackTrace(exception);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(exception.GetType().FullName, message,
                                                                                  stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(1, taskExceptions.Length);

            Assert.Equal(afterExceptions[0].GetType().FullName, taskExceptions[0].Type);
            Assert.Equal(afterExceptions[0].Message, taskExceptions[0].Message);
            Assert.Equal(afterExceptions[0].StackTrace, taskExceptions[0].StackTrace);
            Assert.Equal("AfterTestException: One or more exceptions were thrown from After methods during test cleanup", simplifiedMessage);
        }

        [Fact]
        public void MultipleAfterTestExceptionsConvertedInReverseThrownOrder()
        {
            var afterExceptions = new Exception[3];
            afterExceptions[0] = GenerateSingleException();
            afterExceptions[1] = GenerateSingleException2();
            afterExceptions[2] = GenerateSingleException3();
            var exception = new AfterTestException(afterExceptions);

            string message = ExceptionUtility.GetMessage(exception);
            string stackTrace = ExceptionUtility.GetStackTrace(exception);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(exception.GetType().FullName, message,
                                                                                  stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(3, taskExceptions.Length);

            Assert.Equal(afterExceptions[0].GetType().FullName, taskExceptions[2].Type);
            Assert.Equal(afterExceptions[0].Message, taskExceptions[2].Message);
            Assert.Equal(afterExceptions[0].StackTrace, taskExceptions[2].StackTrace);

            Assert.Equal(afterExceptions[1].GetType().FullName, taskExceptions[1].Type);
            Assert.Equal(afterExceptions[1].Message, taskExceptions[1].Message);
            Assert.Equal(afterExceptions[1].StackTrace, taskExceptions[1].StackTrace);

            Assert.Equal(afterExceptions[2].GetType().FullName, taskExceptions[0].Type);
            Assert.Equal(afterExceptions[2].Message, taskExceptions[0].Message);
            Assert.Equal(afterExceptions[2].StackTrace, taskExceptions[0].StackTrace);

            Assert.Equal("AfterTestException: One or more exceptions were thrown from After methods during test cleanup", simplifiedMessage);
        }

        // Otherwise known as the Moq.MockException test!
        [Fact]
        public void ExceptionMessageBeginningWithNewLineIsCaptured()
        {
            const string expectedMessage = "This is my message";
            Exception singleException = GenerateSingleException(Environment.NewLine + expectedMessage);

            string message = ExceptionUtility.GetMessage(singleException);
            string stackTrace = ExceptionUtility.GetStackTrace(singleException);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(singleException.GetType().FullName, message, stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(1, taskExceptions.Length);
            Assert.Equal(singleException.GetType().FullName, taskExceptions[0].Type);
            Assert.Equal(expectedMessage, taskExceptions[0].Message);
            Assert.Equal(singleException.StackTrace, taskExceptions[0].StackTrace);
        }

        // Ladies and Gentlemen, the Moq.MockVerificationException test!
        [Fact]
        public void ExceptionMessageContainingNewLineIsCaptured()
        {
            string expectedMessage = "This is my " + Environment.NewLine + "message";
            Exception singleException = GenerateSingleException(expectedMessage);

            string message = ExceptionUtility.GetMessage(singleException);
            string stackTrace = ExceptionUtility.GetStackTrace(singleException);

            string simplifiedMessage;
            TaskException[] taskExceptions = ExceptionConverter.ConvertExceptions(singleException.GetType().FullName, message, stackTrace, out simplifiedMessage);

            Assert.NotNull(taskExceptions);
            Assert.Equal(1, taskExceptions.Length);
            Assert.Equal(singleException.GetType().FullName, taskExceptions[0].Type);
            Assert.Equal(expectedMessage, taskExceptions[0].Message);
            Assert.Equal(singleException.StackTrace, taskExceptions[0].StackTrace);
        }

        private static Exception GenerateAssertException()
        {
            try
            {
                Assert.Equal(1, 3);
            }
            catch (Exception exception)
            {
                return exception;
            }
            return null;
        }

        private static Exception GenerateSingleException()
        {
            return GenerateSingleException("message from NotImplementedException");
        }

        private static Exception GenerateSingleException(string message)
        {
            try
            {
                throw new NotImplementedException(message);
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static Exception GenerateSingleException2()
        {
            try
            {
                throw new NotImplementedException("message from NotImplementedException no.2");
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static Exception GenerateSingleException3()
        {
            try
            {
                throw new NotImplementedException("message from NotImplementedException no.3");
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static Exception GenerateNestedExceptions()
        {
            try
            {
                CallMethod1();
            }
            catch (Exception exception)
            {
                return exception;
            }

            return null;
        }

        private static Exception GenerateSingleTargetInvocationException()
        {
            try
            {
                throw new TargetInvocationException("this is the target invocation message", null);
            }
            catch(Exception exception)
            {
                return exception;
            }
        }

        private static Exception GenerateTargetInvocationException()
        {
            try
            {
                CallMethod1();
            }
            catch (Exception exception)
            {
                return new TargetInvocationException("this is a target invocation exception", exception);
            }

            return null;
        }

        private static void CallMethod1()
        {
            try
            {
                CallMethod2();
            }
            catch (Exception exception)
            {
                throw new InvalidProgramException("outermost exception message", exception);
            }
        }

        private static void CallMethod2()
        {
            try
            {
                CallMethod3();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("nested exception message", exception);
            }
        }

        private static void CallMethod3()
        {
            throw new NotImplementedException("innermost exception message");
        }
    }
}