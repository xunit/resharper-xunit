using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Microsoft.Silverlight.Testing;
using Xunit;
using Xunit.Sdk;
using System.Linq;

namespace test.xunit.silverlight
{
    //[Exclusive]
    public class XunitFixesTests
    {
        // ExceptionUtility.RethrowWithNoStackTraceLoss doesn't work on Silverlight
        // because Silverlight only allows reflection on members that you already have
        // access to, and Rethrow... tries to set a private field.
        [Fact]
        [Exclusive]
        public void RethrowWithNoStackTraceLossIsCommentedOut()
        {
            var exception = new Exception();

            // If this test fails, comment out the complete contents of Rethrow... with #if !SILVERLIGHT
            // Then you *MUST* add a throw; after where RethrowWithNoStackTraceLoss is called. Or else ALL tests
            // will look like they're passing!
            // (And that means we can't write a test to ensure that failing tests are logged!!)
            Assert.DoesNotThrow(() => ExceptionUtility.RethrowWithNoStackTraceLoss(exception));
        }

        [Fact]
        [Exclusive]
        public void ExceptionThrownWhenInvokingMethodIsStillThrownAfterCommentingOutRethrowWithNoStackLoss()
        {
            var method = typeof(ReflectorTests.Invoke.TestMethodCommandClass).GetMethod("ThrowsException");
            var wrappedMethod = Reflector.Wrap(method);
            var obj = new ReflectorTests.Invoke.TestMethodCommandClass();

            Exception ex = Record.Exception(() => wrappedMethod.Invoke(obj));

            if (ex == null)
            {
                // If you see this message, it means that exceptions aren't getting propogated out of the
                // unit test system. If you've commented out RethroWithNoStackTraceLoss above, then you need
                // to add a "throw;" after where Rethrow... was being called - for this test, that means
                // Reflector.ReflectionMethodInfo.Invoke. It should look like:
                //
                // catch (TargetInvocationException ex)
                // {
                //     ExceptionUtility.RethrowWithNoStackTraceLoss(ex.InnerException);
                //     throw;  // <---- New line
                // }
                //
                // This is the only test that displays a message box - if this test fails, it means that
                // the system isn't reporting failures, so we need to resort to something drastic to get
                // it working. Once this test works, other failures will be reported accurately.
                // Arguably, this could be split into two tests - one that the xunitcontrib provider is
                // properly reporting failures, and this method to assert that the correct changes are
                // applied to xunit

                var thisMethod = new StackTrace().GetFrame(0).GetMethod();
                MessageBox.Show(string.Format("Failing tests are not being reported{0}{0}TargetInvocationException not being rethrown{0}See {1}.{2} for more details",
                    Environment.NewLine, thisMethod.DeclaringType.Name, thisMethod.Name));
                throw new ExceptionNotBeingRethrownException("Reflector.ReflectionMethodInfo.Invoke");
            }

            Assert.NotNull(ex);
            Assert.IsType<TargetInvocationException>(ex);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        [Exclusive]
        public void ExceptionsThrownWhileSettingFixturesProperlyReported()
        {
            var testClassCommand = new TestClassCommand(Reflector.Wrap(typeof (SetFixtureFailureSpy)));
            testClassCommand.ClassStart();

            var method = testClassCommand.TypeUnderTest.GetMethod("NotReallyATestMethod");
            var testCommands = TestCommandFactory.Make(testClassCommand, method);

            // There should be only one test command for this method
            var methodResult = testCommands.Single().Execute(null);
            if (!(methodResult is FailedResult))
                throw new ExceptionNotBeingRethrownException("FixtureCommand.Execute");

            var failedResult = (FailedResult) methodResult;
            Assert.Equal("System.Reflection.TargetInvocationException", ((FailedResult)methodResult).ExceptionType);

            testClassCommand.ClassFinish();

#if false
            
            
            
            
            var method = typeof(SetFixtureFailureSpy).GetMethod("NotReallyATestMethod");
            var wrappedMethod = Reflector.Wrap(method);
            var obj = new SetFixtureFailureSpy();

            Exception ex = Record.Exception(() => wrappedMethod.Invoke(obj));

            // If this test fails, the TargetInvocationException being caught by FixtureCommand
            // needs to be rethrown (because we commented out RethrowWithNoStackTraceLoss). It
            // should look like this:
            //
            // catch (TargetInvocationException ex)
            // {
            //     ExceptionUtility.RethrowWithNoStackTraceLoss(ex.InnerException);
            //     throw;  // <---- New line
            // }

            if (ex == null)
            {
                throw new ExceptionNotBeingRethrownException("FixtureCommand.Execute");
            }

            Assert.NotNull(ex);
            Assert.IsType<TargetInvocationException>(ex);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
#endif
        }

        public class ExceptionNotBeingRethrownException : Exception
        {
            public ExceptionNotBeingRethrownException(string where) : base(string.Format("Expected TargetInvocationException to be rethrown at {0}", where))
            {
            }
        }
        
        public class SetFixtureFailureSpy : FixtureSpy, IUseFixture<Data>
        {
            [Fact]
            public void NotReallyATestMethod()
            {
            }

            public void SetFixture(Data data)
            {
                throw new InvalidOperationException();
            }
        }

        public class Data
        {
        }
    }
}